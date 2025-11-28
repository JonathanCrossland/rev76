using Assetto.Data.Broadcasting;
using Assetto.Data.Broadcasting.Structs;
using ExCSS;
using Rev76.DataModels;
using Rev76.Windows.Rendering;
using Svg;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

using Color = System.Drawing.Color;

namespace Rev76.Windows.Widgets
{
    public class LeaderboardWidget : OverlayWindow
    {
        //private SVGRenderer _renderer = new SVGRenderer();
        private DirectXRenderer _renderer = new DirectXRenderer();

        private bool _DriversAdded = false;
        private bool _CreatingLeaderboard = false;
        private bool _InRender = false;
        private List<Car> carList = new List<Car>();
        private DateTime _lastCarListUpdate = DateTime.MinValue;
        private const int CAR_LIST_UPDATE_INTERVAL_MS = 20; // Update car list every 20ms
        private int _lastCarCount = 0; // Track the last car count to avoid unnecessary updates
        private Dictionary<int, int> _lastCarPositions = new Dictionary<int, int>(); // Track positions for change detection
        
        private DateTime _incompleteDataDetectedTime = DateTime.MinValue;
        private bool _incompleteDataDetected = false;
        private const int INCOMPLETE_DATA_REFRESH_THRESHOLD_MS = 30000; // 30 seconds

        // Color cache to reuse SvgColourServer objects
        private Dictionary<Color, SvgColourServer> _colorCache = new Dictionary<Color, SvgColourServer>();
        private Dictionary<DriverCategory, SvgColourServer> _licenseColorCache = new Dictionary<DriverCategory, SvgColourServer>();
        private Dictionary<CarClass, SvgColourServer> _classColorCache = new Dictionary<CarClass, SvgColourServer>();

        // Common colors used in the leaderboard
        private SvgColourServer _whiteColor;
        private SvgColourServer _blackColor;
        private SvgColourServer _redColor;
        private SvgColourServer _orangeColor;
        private SvgColourServer _lightYellowColor;
        private SvgColourServer _darkOrangeColor;
        private SvgColourServer _greenColor;
        private SvgColourServer _purpleColor;
        private SvgColourServer _silverColor;
        private SvgColourServer _goldColor;
        private SvgColourServer _saddleBrownColor;
        private SvgColourServer _whiteTextColor;
        private SvgColourServer _blackTextColor;

        // Cache for SVG elements to avoid repeated OfType<T>().FirstOrDefault() calls
        private Dictionary<string, Dictionary<string, SvgElement>> _elementCache = new Dictionary<string, Dictionary<string, SvgElement>>();
        
        // Template caching
        private SvgGroup _cachedTemplate = null;
        private int _cachedTemplateHeight = 0;
        private int _lastDisplayCarCount = 0;
        private int _lastHeight = 0;

        // Text caching for header elements
        private string _cachedRaceType = null;
        private string _cachedTime = null;
        private string _cachedLaps = null;
        private DateTime _lastTimeUpdate = DateTime.MinValue;
        private const int TIME_UPDATE_INTERVAL_MS = 100; // Update time every 100ms
        private bool _lastChequeredFlag = false;

        // Frame-based rendering control
        private DateTime _lastHeaderRender = DateTime.MinValue;
        private DateTime _lastDriverRender = DateTime.MinValue;
        private const int HEADER_RENDER_INTERVAL_MS = 1000; // Render headers every 1 second
        private const int DRIVER_RENDER_INTERVAL_MS = 100; // Render drivers every 100ms
        private int _currentRenderFrame = 0; // Track which frame we're on

        public LeaderboardWidget(int x, int y, int width, int height, float scale, Icon icon) : base(x, y, width, height, scale, icon)
        {
            // Initialize common colors
            _whiteColor = GetCachedColor(Color.White);
            _blackColor = GetCachedColor(Color.Black);
            _redColor = GetCachedColor(Color.Red);
            _orangeColor = GetCachedColor(Color.Orange);
            _lightYellowColor = GetCachedColor(Color.FromArgb(255, 255, 200, 0));
            _darkOrangeColor = GetCachedColor(Color.DarkOrange);
            _greenColor = GetCachedColor(Color.Green);
            _purpleColor = GetCachedColor(Color.FromArgb(255, 189, 0, 236));
            _silverColor = GetCachedColor(Color.Silver);
            _goldColor = GetCachedColor(Color.Gold);
            _saddleBrownColor = GetCachedColor(Color.SaddleBrown);
            _whiteTextColor = GetCachedColor(Color.FromArgb(255, 255, 255, 255));
            _blackTextColor = GetCachedColor(Color.FromArgb(255, 0, 0, 0));
        }

        // Helper method to get a cached color
        private SvgColourServer GetCachedColor(Color color)
        {
            if (!_colorCache.TryGetValue(color, out var svgColor))
            {
                svgColor = new SvgColourServer(color);
                _colorCache[color] = svgColor;
            }
            return svgColor;
        }

        // Helper method to get a cached element from a group
        private T GetCachedElement<T>(SvgGroup group, string className) where T : SvgElement
        {
            string groupId = group.ID ?? "unknown";
            
            if (!_elementCache.TryGetValue(groupId, out var elementDict))
            {
                elementDict = new Dictionary<string, SvgElement>();
                _elementCache[groupId] = elementDict;
            }
            
            if (!elementDict.TryGetValue(className, out var element))
            {
                // Find the element and cache it
                foreach (var child in group.Children)
                {
                    if (child.CustomAttributes.TryGetValue("class", out var value) && value == className)
                    {
                        element = child;
                        elementDict[className] = element;
                        Debug.WriteLine($"Found and cached element {className} in group {groupId}");
                        break;
                    }
                }
            }
            
            // If we still don't have the element, try to find it directly
            if (element == null)
            {
                element = group.Children.OfType<T>().FirstOrDefault(e => 
                    e.CustomAttributes.TryGetValue("class", out var value) && value == className);
                
                if (element != null)
                {
                    elementDict[className] = element;
                    Debug.WriteLine($"Found element {className} directly in group {groupId}");
                }
                else
                {
                    Debug.WriteLine($"Could not find element {className} in group {groupId}");
                }
            }
            
            return element as T;
        }

        public override string Title => "Leaderboard";

        public override bool Visible { get => 
                GameData.Snapshot.GameState.Status == GameStatus.LIVE
                &&
                GameData.Snapshot.GameState.IsSetupMenuVisible == false;
        }

       
        protected override void OnRender(System.Drawing.Graphics gfx)
        {
            if (GameData.Snapshot.GameState.Status != GameStatus.LIVE)
            {
                return;
            }
          
            try
            {
                if (_InRender == true) return;

                _InRender = true;

                // Check if any positions have changed
                bool positionsChanged = HasPositionChanged();
                
                // Only update car list if the count has changed, positions changed, or enough time has passed
                int currentCarCount = GameData.Snapshot.Track.Cars.Count;
                if (currentCarCount != _lastCarCount || 
                    positionsChanged || 
                    (DateTime.Now - _lastCarListUpdate).TotalMilliseconds >= CAR_LIST_UPDATE_INTERVAL_MS)
                {
                    // Instead of clearing and rebuilding, update the existing list
                    UpdateCarList(currentCarCount);
                    
                    _lastCarListUpdate = DateTime.Now;
                    _lastCarCount = currentCarCount;
                    
                    // Update position tracking
                    UpdatePositionTracking();
                }

                // Use the full car list instead of limiting to 8 cars
                List<Car> displayCarList = carList;

                SvgGroup template = null;

                // Determine what to render in this frame
                bool renderHeaders = (DateTime.Now - _lastHeaderRender).TotalMilliseconds >= HEADER_RENDER_INTERVAL_MS;
                bool renderDrivers = (DateTime.Now - _lastDriverRender).TotalMilliseconds >= DRIVER_RENDER_INTERVAL_MS;
                
                // Update the current render frame
                _currentRenderFrame = (_currentRenderFrame + 1) % 3; // Cycle through 3 frames

                try 
                {
                    Trace.WriteLine($"Starting DrawSvg with {displayCarList.Count} cars, renderHeaders: {renderHeaders}, renderDrivers: {renderDrivers}");
                    
                    bool success = _renderer.DrawSvg
                (
                    gfx,
                    0,
                        0, 0, 500 * Scale, 850 * Scale,
                    element =>
                        {
                            try
                            {
                                if (element == null)
                                {
                                    Trace.WriteLine("DrawSvg callback received null element");
                                    return true;
                                }

                                Trace.WriteLine($"Processing element ID: {element.ID}, Type: {element.GetType().Name}");
                                
                                // Process text elements (headers)
                                if (element is SvgText text)
                                {
                                    if (!renderHeaders) return false; // Skip header updates if not time

                                    switch (text.ID)
                                    {
                                        case "racetype":
                                            // Only update race type if it has changed
                                            string currentRaceType = GameData.Snapshot.Session.SessionType.ToString().ToUpper();
                                            if (_cachedRaceType != currentRaceType)
                                            {
                                                text.Text = currentRaceType;
                                                _cachedRaceType = currentRaceType;
                                            }
                                            return true;
                                        case "time":
                                            // Only update time if enough time has passed or flag status changed
                                            bool currentChequeredFlag = GameData.Snapshot.Session.Flag == FlagType.CHECKERED_FLAG;
                                            if ((DateTime.Now - _lastTimeUpdate).TotalMilliseconds >= TIME_UPDATE_INTERVAL_MS || 
                                                _lastChequeredFlag != currentChequeredFlag)
                                            {
                                                string currentTime = GameData.FormatTimeLeft(GameData.Snapshot.Session.SessionTimeLeft);
                                                if (_cachedTime != currentTime)
                                                {
                                                    text.Text = currentTime;
                                                    _cachedTime = currentTime;
                                                }
                                                _lastTimeUpdate = DateTime.Now;
                                                _lastChequeredFlag = currentChequeredFlag;
                                            }
                                            return true;
                                        case "laps":
                                            // Only update laps if it has changed
                                            string currentLaps = $"Lap {GameData.Snapshot.Session.CompletedLaps}";
                                            if (_cachedLaps != currentLaps)
                                            {
                                                text.Text = currentLaps;
                                                _cachedLaps = currentLaps;
                                            }
                                            return true;
                                        default:
                                            return true;
                                    }
                                }
                                
                                // Process rectangle elements (flag)
                                if (element is SvgRectangle rect && rect.ID == "flag")
                                {
                                    if (!renderHeaders) return false; // Skip flag updates if not time

                                if (GameData.Snapshot.Session.GlobalChequered == 1)
                                {
                                    rect.Fill = new SvgDeferredPaintServer("url(#chequeredFlag)");
                                    rect.Visibility = "visible";
                                }
                                else
                                {
                                    rect.Visibility = "hidden";
                                }
                                    return true;
                            }
                         
                                // Process template element
                        if (element.ID == "template")
                        {
                                    // Use cached template if available
                                    if (_cachedTemplate == null)
                                    {
                                        _cachedTemplate = element.Clone();
                                        // Calculate the exact height from the template's bounds
                                        _cachedTemplateHeight = (int)Math.Ceiling(_cachedTemplate.Bounds.Height);
                                        Trace.WriteLine($"Cached template height: {_cachedTemplateHeight}");
                                    }
                                    
                                    template = _cachedTemplate;
                                    
                                    // Add red background to the template after cloning
                                    var background = new SvgRectangle
                                    {
                                        Width = new SvgUnit(450),
                                        Height = new SvgUnit(26),
                                        Fill = new SvgColourServer(Color.FromArgb(128, 255, 0, 0))
                                    };
                                    element.Children.Insert(0, background);
                                    
                                    // Keep the template hidden
                                    element.Visibility = "hidden";
                                    
                                    // Only update height if the car count has changed
                                    if (_lastDisplayCarCount != displayCarList.Count)
                                    {
                                        // Add extra padding to ensure all cars are visible
                                        int extraPadding = 20; // Add 20 pixels of extra padding
                                        this.Height = 200 + (displayCarList.Count * _cachedTemplateHeight) + extraPadding;
                                        _lastDisplayCarCount = displayCarList.Count;
                                        _lastHeight = this.Height;
                                        
                                        // Force a redraw by setting a flag
                                        _DriversAdded = false;
                                        Trace.WriteLine($"Updated height for {displayCarList.Count} cars: {this.Height}");
                                }
                                else
                                {
                                        // Use cached height
                                        this.Height = _lastHeight;
                                    }
                                    return true;
                                }

                                // Process driver rows
                                if (element.ID == "DriverRows")
                                {
                                    if (!renderDrivers) return false; // Skip driver updates if not time

                                    if (renderDrivers)
                                    {
                                        InitDrivers(element, template, displayCarList);
                                        _lastDriverRender = DateTime.Now;
                                    }
                                    return true;
                                }

                                // Process car elements
                                if (element.ID != null && element.ID.StartsWith("car_"))
                                {
                                    if (!renderDrivers) return false; // Skip car updates if not time
                                    
                                    var positionString = (element as SvgGroup).ID.Replace("car_", "");
                                    int.TryParse(positionString, out int rowNumber);
                                    
                                    // Use list index instead of position lookup
                                    int listIndex = rowNumber - 1; // Convert to 0-based index
                                    if (listIndex < 0 || listIndex >= displayCarList.Count)
                                    {
                                        return true; // Skip if no car for this row
                                    }
                                    
                                    Car car = displayCarList[listIndex];
                                    if (car != null)
                                    {
                                        SvgGroup row = element as SvgGroup;
                                        if (row == null) return true;  // Skip if row conversion failed
                                        
                                        // Show row even if position is 0, but log it
                                        if (car.Position <= 0)
                                        {
                                            Trace.WriteLine($"Warning: Car at listIndex {listIndex} has invalid position: {car.Position}, CarIndex: {car.CarIndex}");
                                        }
                                        
                                        row.Visibility = "visible";

                                        // Lap times processing in its own try-catch
                                        try
                                        {
                                            // Get last lap times for this car
                                            var laptimes = new ConcurrentBag<LapInfo>(car.LapTimes != null ? car.LapTimes.Values : new List<LapInfo>());
                                            var lastLaps = laptimes
                                                .Where(lap => lap != null && lap.LaptimeMS > 0)
                                                .OrderByDescending(lap => lap != null ? lap.LapNumber : 0)
                                                .Take(3)
                                                .ToList();

                                            Debug.WriteLine($"Processing lap times for car {car.Position}: Found {lastLaps.Count} laps");
                                            
                                            // Process the lap time rectangles
                                            for (int i = 0; i < lastLaps.Count; i++)
                                            {
                                                string rectId = $"driverrect{i + 1}";
                                                
                                                // Find the rectangle directly in the row's children
                                                SvgRectangle lapRect = row.Children.OfType<SvgRectangle>()
                                                    .FirstOrDefault(r => r.CustomAttributes.TryGetValue("class", out var value) && value == rectId);
                                                
                                                if (lapRect != null)
                                                {
                                                    Debug.WriteLine($"Found rectangle {rectId} for car {car.Position}");
                                                    
                                                    if (lastLaps.Count > i && lastLaps[i] != null)
                                                    {
                                                        Debug.WriteLine($"Processing lap {i + 1} for car {car.Position}: Time={lastLaps[i].LaptimeMS}, Invalid={lastLaps[i].IsInvalid}");
                                                        
                                                        // Compare with next lap if available
                                                        if (lastLaps.Count > i + 1 && lastLaps[i + 1] != null)
                                                        {
                                                            if (lastLaps[i].LaptimeMS > lastLaps[i + 1].LaptimeMS)
                                                            {
                                                                lapRect.Fill = new SvgColourServer(Color.FromArgb(255, 240, 0, 0));
                                                                Debug.WriteLine($"Car {car.Position} Lap {i + 1}: SLOWER than previous");
                                }
                                else
                                {
                                                                lapRect.Fill = new SvgColourServer(Color.FromArgb(255, 0, 200, 0));
                                                                Debug.WriteLine($"Car {car.Position} Lap {i + 1}: FASTER than previous");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            lapRect.Fill = new SvgColourServer(Color.FromArgb(255, 0, 200, 0));
                                                            
                                                        }

                                                        // If this is the best lap of the session
                                                        if (car.BestSessionLap != null && lastLaps[i].LaptimeMS == car.BestSessionLap.LaptimeMS)
                                                        {
                                                            lapRect.Fill = new SvgColourServer(Color.FromArgb(255, 148, 0, 185));
                                                            Debug.WriteLine($"Car {car.Position} Lap {i + 1}: BEST LAP");
                                                        }
                                                        
                                                        // Invalid lap
                                                        if (lastLaps[i].IsInvalid)
                                                        {
                                                            lapRect.Fill = new SvgColourServer(Color.FromArgb(255, 249, 131, 4));
                                                            Debug.WriteLine($"Car {car.Position} Lap {i + 1}: INVALID LAP");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // No lap data available, set default color
                                                        lapRect.Fill = new SvgColourServer(Color.FromArgb(255, 34, 34, 34));
                                                        Debug.WriteLine($"Car {car.Position} Lap {i + 1}: NO LAP DATA");
                                                    }
                                                }
                                                else
                                                {
                                                    Debug.WriteLine($"Could not find rectangle {rectId} for car {car.Position}");
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine($"Error processing lap times for car {car.Position}: {ex.Message}");
                                        }

                                        // Driver info processing in its own try-catch
                                        try
                                        {
                                            SvgText drivername = GetCachedElement<SvgText>(row, "drivername");
                                            if (drivername != null)
                                            {
                                                if (car.Drivers != null && car.Drivers.Count > 0 &&
                                                    car.DriverIndex >= 0 && car.DriverIndex < car.Drivers.Count)
                                                {
                                                    DriverInfo driver;
                                                    if (car.Drivers.TryGetValue(car.DriverIndex, out driver))
                                                    {
                                                        string firstNameInitial = !string.IsNullOrEmpty(driver.FirstName) ? 
                                                            driver.FirstName[0].ToString().ToUpper() : "";
                                                        drivername.Text = $"{firstNameInitial} {driver.LastName ?? ""}".Trim();
                                                    }
                                                    else
                                                    {
                                                        drivername.Text = "--";
                                                    }
                                                }
                                                else
                                                {
                                                    drivername.Text = "--";
                                                }
                                            }

                                            SvgRectangle numberRect = GetCachedElement<SvgRectangle>(row, "numberrect");
                                            if (numberRect != null && car.Drivers != null && car.Drivers.Count > 0 && 
                                                car.DriverIndex >= 0 && car.DriverIndex < car.Drivers.Count)
                                            {
                                                DriverInfo driver;
                                                if (car.Drivers.TryGetValue(car.DriverIndex, out driver))
                                                {
                                                    var category = driver.Category;
                                                    numberRect.Fill = GetFillForDriverLicense(category);
                                                }
                                                else
                                                {
                                                    numberRect.Fill = GetFillForDriverLicense(DriverCategory.Error);
                                                }
                                            }

                                            SvgText number = GetCachedElement<SvgText>(row, "number");
                                            if (number != null)
                                            {
                                                if (car.Number > 0)
                                                {
                                                    number.Text = car.Number.ToString();
                                                }
                                                else
                                                {
                                                    number.Text = "";
                                                    Trace.WriteLine($"Car at position {car.Position} (listIndex {listIndex}) has no number. CarIndex: {car.CarIndex}");
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Trace.WriteLine($"Error processing driver info for car {car.Position}: {ex.Message}");
                                        }

                                        // Last lap and gap calculation in its own try-catch
                                        try
                                        {
                                            SvgText lastlaptime = GetCachedElement<SvgText>(row, "lastlaptime");
                                            if (lastlaptime != null)
                                            {
                                                float time = car.LastLap != null && float.TryParse(car.LastLap.LaptimeMS.ToString(), out time) ? time : 0;
                                lastlaptime.Text = GameData.GetFormattedLapTime(time);
                                            }

                                            // Get the car ahead (previous index in sorted list)
                                            Car postCar = listIndex > 0 ? displayCarList[listIndex - 1] : null;
                                            if (postCar != null)
                                {
                                                SvgText offsettime = GetCachedElement<SvgText>(row, "offsettime");
                                                if (offsettime != null)
                                                {
                                    offsettime.Text = "";
                                    if (postCar.Laps > car.Laps)
                                    {
                                        offsettime.Text = $"+{postCar.Laps - car.Laps} Laps";
                                                        offsettime.Fill = _orangeColor;
                                    }
                                    else
                                    {
                                        float postcarGap = Car.CalculateTimeGap(postCar, car, GameData.Snapshot.Track.TrackLength);
                                        if (postcarGap > 0)
                                        {
                                            offsettime.Text = $"+{GameData.GetFormattedGap(postcarGap)}";
                                        }
                                                        offsettime.Fill = _lightYellowColor;
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine($"Error processing lap times and gaps for car {car.Position}: {ex.Message}");
                                        }

                                        // Pit info processing in its own try-catch
                                        try
                                        {
                                            SvgGroup pit = GetCachedElement<SvgGroup>(row, "pit");
                                            if (pit != null)
                                            {
                                                // Get the pit rectangle and text directly from the pit group
                                                SvgRectangle pitRect = null;
                                                SvgText pitText = null;
                                                
                                                // Only do this once per pit group
                                                if (!_elementCache.TryGetValue(pit.ID ?? "pit", out var pitElements))
                                                {
                                                    pitElements = new Dictionary<string, SvgElement>();
                                                    _elementCache[pit.ID ?? "pit"] = pitElements;
                                                    
                                                    // Find the rectangle and text elements
                                                    foreach (var child in pit.Children)
                                                    {
                                                        if (child is SvgRectangle rectElement)
                                                        {
                                                            pitRect = rectElement;
                                                            pitElements["rect"] = pitRect;
                                                        }
                                                        else if (child is SvgText textElement)
                                                        {
                                                            pitText = textElement;
                                                            pitElements["text"] = pitText;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    // Use cached elements
                                                    pitRect = pitElements.TryGetValue("rect", out var rectElement) ? rectElement as SvgRectangle : null;
                                                    pitText = pitElements.TryGetValue("text", out var textElement) ? textElement as SvgText : null;
                                                }

                                                if (pitRect != null && pitText != null)
                                                {
                                                    pit.Visibility = "visible";  // Always visible now
                                                    pitText.Text = "";  // Always start with empty text
                                                    
                                                    if (GameData.Snapshot.Session != null && 
                                                        GameData.Snapshot.Session.MissingMandatoryPits == 1 && 
                                                        GameData.Snapshot.Session.MandatoryPitDone == 0)
                                                    {
                                                        pitRect.Fill = _darkOrangeColor;
                                    pitText.Text = "1";
                                }
                                                    else if (car.InPits)
                                {
                                                        pitRect.Fill = _whiteColor;
                                    pitText.Text = "P";
                                }
                                else
                                {
                                                        pitRect.Fill = _greenColor;
                                                        // Text remains empty string
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Trace.WriteLine($"Error processing pit info for car {car.Position}: {ex.Message}");
                                        }

                                        // Use cached elements instead of OfType<T>().FirstOrDefault()
                                        SvgText pos = GetCachedElement<SvgText>(row, "pos");
                                        if (pos != null)
                                        {
                                            // Display the row number (1-based) since list is already sorted by position
                                            pos.Text = (listIndex + 1).ToString();

                                            if (GameData.Snapshot.Session?.BestSession != null && 
                                                car.CarIndex == GameData.Snapshot.Session.BestSession.CarIndex)
                                            {
                                                pos.Fill = GetCachedColor(Color.FromArgb(255, 255, 255, 236));
                                            }
                                            else if (car.CarIndex == GameData.Snapshot.PlayerCarIndex || 
                                                   (GameData.Snapshot.BroadcastCar != null && car.CarIndex == GameData.Snapshot.BroadcastCar.CarIndex))
                                            {
                                                pos.Fill = _whiteTextColor;
                                            }
                                            else
                                            {
                                                pos.Fill = _blackTextColor;
                                            }
                                        }

                                        SvgRectangle posRect = GetCachedElement<SvgRectangle>(row, "posrect");
                                        if (posRect != null)
                                        {
                                            if (GameData.Snapshot.Session?.BestSession != null && 
                                                car.CarIndex == GameData.Snapshot.Session.BestSession.CarIndex)
                                            {
                                                posRect.Fill = _purpleColor;
                                            }
                                            else if (car.CarIndex == GameData.Snapshot.PlayerCarIndex || 
                                                   (GameData.Snapshot.BroadcastCar != null && car.CarIndex == GameData.Snapshot.BroadcastCar.CarIndex))
                                            {
                                                posRect.Fill = _redColor;
                                            }
                                            else
                                            {
                                                posRect.Fill = _whiteColor;
                                            }
                                        }
                                    }
                                    return true;
                                }
                                
                                // For any other elements, skip processing
                                return true;
                            }
                            catch (Exception callbackEx)
                            {
                                Trace.WriteLine($"Error in DrawSvg callback for element {element?.ID}: {callbackEx.Message}");
                                Trace.WriteLine($"Stack trace: {callbackEx.StackTrace}");
                                return true; // Continue processing other elements
                            }
                        }
                    );

                    Trace.WriteLine($"DrawSvg completed with success: {success}");
                    
                    // Update the last header render time if we rendered headers
                    if (renderHeaders)
                    {
                        _lastHeaderRender = DateTime.Now;
                    }
                }
                catch (Exception renderEx)
                {
                    Trace.WriteLine($"Error during DrawSvg: {renderEx.Message}");
                    Trace.WriteLine($"Stack trace: {renderEx.StackTrace}");
                }
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine($"Leaderboard: {ex.Message}");
            }
            finally
            {
                base.OnRender(gfx);
                _InRender = false;
            }
        }

        private bool HasPositionChanged()
        {
            foreach (var car in GameData.Snapshot.Track.Cars.Values)
            {
                if (_lastCarPositions.TryGetValue(car.CarIndex, out int lastPosition))
                {
                    if (lastPosition != car.Position)
                    {
                        return true;
                    }
                }
                else
                {
                    // New car that wasn't tracked before
                    return true;
                }
            }
            return false;
        }

        private void UpdatePositionTracking()
        {
            _lastCarPositions.Clear();
            foreach (var car in GameData.Snapshot.Track.Cars.Values)
            {
                _lastCarPositions[car.CarIndex] = car.Position;
            }
        }

        // Optimized method to update the car list without clearing and rebuilding
        private void UpdateCarList(int currentCarCount)
        {
            // If the list is empty or too small, initialize it
            if (carList.Count == 0 || carList.Capacity < currentCarCount)
            {
                carList.Clear();
                carList.Capacity = currentCarCount;
                foreach (var car in GameData.Snapshot.Track.Cars.Values)
                {
                    carList.Add(car);
                }
                return;
            }
            
            // Create a dictionary of existing cars by CarIndex for quick lookup
            Dictionary<int, Car> existingCars = new Dictionary<int, Car>();
            foreach (var car in carList)
            {
                existingCars[car.CarIndex] = car;
            }
            
            // Track which cars are still present
            HashSet<int> currentCarIndices = new HashSet<int>();
            
            // Track incomplete data
            bool hasIncompleteData = false;
            
            // Update or add cars
            foreach (var car in GameData.Snapshot.Track.Cars.Values)
            {
                currentCarIndices.Add(car.CarIndex);
                
                if (existingCars.TryGetValue(car.CarIndex, out Car existingCar))
                {
                    // Update existing car properties
                    existingCar.Position = car.Position;
                    existingCar.Laps = car.Laps;
                    existingCar.LastLap = car.LastLap;
                    existingCar.InPits = car.InPits;
                    existingCar.DriverIndex = car.DriverIndex;
                    existingCar.BestSessionLap = car.BestSessionLap;
                    existingCar.TrackPosition = car.TrackPosition;
                    existingCar.Kmh = car.Kmh;
                    existingCar.Flag = car.Flag;
                    existingCar.Number = car.Number;
                    existingCar.CarClass = car.CarClass;

                    // Update drivers collection
                    if (car.Drivers != null && car.Drivers.Count > 0)
                    {
                        foreach (var driver in car.Drivers)
                        {
                            existingCar.Drivers[driver.Key] = driver.Value;
                        }
                    }

                    // Update lap times collection
                    foreach (var lap in car.LapTimes)
                    {
                        if (!existingCar.LapTimes.ContainsKey(lap.Key))
                        {
                            existingCar.LapTimes[lap.Key] = lap.Value;
                        }
                    }
                }
                else
                {
                    // Add new car
                    carList.Add(car);
                }
                
                // Check for incomplete data (missing driver info or number)
                if (car.Position > 0 && (car.Number == 0 || car.Drivers == null || car.Drivers.Count == 0))
                {
                    hasIncompleteData = true;
                }
            }
            
            // Remove cars that are no longer present
            carList.RemoveAll(car => !currentCarIndices.Contains(car.CarIndex));
            
            // Sort the list by position, but push cars with invalid positions to the end
            carList.Sort((a, b) => 
            {
                // Invalid positions go to the end
                if (a.Position <= 0 && b.Position <= 0) return 0;
                if (a.Position <= 0) return 1;  // a goes after b
                if (b.Position <= 0) return -1; // b goes after a
                
                // Normal position comparison
                return a.Position.CompareTo(b.Position);
            });
            
            // Handle incomplete data detection
            CheckIncompleteData(hasIncompleteData);
        }
        
        private void CheckIncompleteData(bool hasIncompleteData)
        {
            if (hasIncompleteData)
            {
                // Start timer if this is the first detection
                if (!_incompleteDataDetected)
                {
                    _incompleteDataDetected = true;
                    _incompleteDataDetectedTime = DateTime.Now;
                    Trace.WriteLine("Incomplete car data detected in leaderboard");
                }
                else
                {
                    // Check if threshold exceeded
                    if ((DateTime.Now - _incompleteDataDetectedTime).TotalMilliseconds >= INCOMPLETE_DATA_REFRESH_THRESHOLD_MS)
                    {
                        Trace.WriteLine("Incomplete data threshold exceeded - requesting entry list refresh");
                        RequestEntryListRefresh();
                        
                        // Reset timer to avoid spamming requests
                        _incompleteDataDetectedTime = DateTime.Now;
                    }
                }
            }
            else
            {
                // Data is complete - reset detection
                if (_incompleteDataDetected)
                {
                    Trace.WriteLine("Car data is now complete");
                    _incompleteDataDetected = false;
                    _incompleteDataDetectedTime = DateTime.MinValue;
                }
            }
        }
        
        private void RequestEntryListRefresh()
        {
            try
            {
                // Invoke the static refresh action on GameData
                GameData.OnRequestEntryListRefresh?.Invoke();
                Trace.WriteLine("Entry list refresh invoked from leaderboard");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error requesting entry list refresh: {ex.Message}");
            }
        }

        private SvgPaintServer GetFillForDriverLicense(DriverCategory category)
        {
            if (_licenseColorCache.TryGetValue(category, out var cachedColor))
            {
                return cachedColor;
            }

            SvgColourServer color;
            switch (category)
            {
                case DriverCategory.Platinum:
                    color = _silverColor;
                    break;
                case DriverCategory.Gold:
                    color = _goldColor;
                    break;
                case DriverCategory.Silver:
                    color = _silverColor;
                    break;
                case DriverCategory.Bronze:
                    color = _saddleBrownColor;
                    break;
                case DriverCategory.Error:
                default:
                    color = _whiteColor;
                    break;
            }
            
            _licenseColorCache[category] = color;
            return color;
        }

        private SvgPaintServer GetFillForDriverClass(CarClass carClass)
        {
            if (_classColorCache.TryGetValue(carClass, out var cachedColor))
            {
                return cachedColor;
            }

            SvgColourServer color = _whiteColor;
            _classColorCache[carClass] = color;
            return color;
        }

        private void InitDrivers(SvgGroup parentGroup, SvgGroup template, List<Car> displayCarList)
        {
            if (template == null) return;
            if (_CreatingLeaderboard) return;

            if (!displayCarList.Any())
            {
                Trace.WriteLine("No cars in display list, returning early");
                return;
            }
            int maxPosition = displayCarList.Max(c => c.Position);
            int currentRowCount = parentGroup.Children.Count;
            
            // Only add new rows if needed - don't clear existing ones
            if (maxPosition > currentRowCount)
            {
                Trace.WriteLine($"Adding more rows. Current: {currentRowCount}, Required: {maxPosition}, Adding: {maxPosition - currentRowCount}");
            }

            // Add drivers if we haven't done so yet or if we need more rows
            if (parentGroup.ID == "DriverRows" && maxPosition >= 1 && (!_DriversAdded || maxPosition > currentRowCount))
            {
                _CreatingLeaderboard = true;
                
                // Fixed height per row - this should match the SVG template height
                const float ROW_HEIGHT = 26f;
                
                // Create rows ONLY for the new positions needed (don't rebuild existing)
                for (int i = currentRowCount; i < maxPosition; i++)
                {
                    // Clone the template and ensure it's a proper SvgGroup
                    SvgGroup row = template.Clone() as SvgGroup;
                    if (row == null)
                    {
                        Trace.WriteLine($"Failed to clone template for position {i + 1}");
                        continue;
                    }

                    // Set visibility and ID
                    row.Visibility = "visible";
                    string carId = $"car_{(i + 1).ToString()}";
                    row.ID = carId;

                    // Add green background to the cloned template for debugging
                    var background = new SvgRectangle
                    {
                        Width = new SvgUnit(450),
                        Height = new SvgUnit(26),
                        Fill = new SvgColourServer(Color.FromArgb(50, 0, 255, 0))
                    };
                    row.Children.Insert(0, background);

                    // Calculate the vertical position
                    float verticalPosition = i * ROW_HEIGHT;
                    
                    // Create transform collection
                    var transforms = new Svg.Transforms.SvgTransformCollection
                    {
                        new Svg.Transforms.SvgTranslate(0, verticalPosition)
                    };
                    row.Transforms = transforms;

                    // Debug logging for positioning
                    Trace.WriteLine($"Creating template row {carId} at position {verticalPosition}");

                    // Add the row to the parent group
                    parentGroup.Children.Add(row);
                }
                
                _DriversAdded = true;
                _CreatingLeaderboard = false;
                
                // Update the height to accommodate all rows
                int extraPadding = 20; // Add 20 pixels of extra padding
                this.Height = (int)(200 + (maxPosition * ROW_HEIGHT) + extraPadding);
                Trace.WriteLine($"Updated height for {maxPosition} positions: {this.Height}");
            }
        }

        protected override void OnGraphicsSetup(System.Drawing.Graphics gfx)
        {
            this._renderer.LoadSvgFiles(
               new List<string>
               {
                    "Assets/Leaderboard.svg",
               });

            base.OnGraphicsSetup(gfx);
        }
    }
}
