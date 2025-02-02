using Rev76.DataModels;
using System.Collections.Generic;
using System.Linq;

namespace Rev76.Windows.Helpers
{
    public static class SplineUtil
    {
        private const float TrackLength = 1.0f; // Assuming spline is normalized (0.0 to 1.0)

        /// <summary>
        /// Gets the car positioned ahead of the given car on the track.
        /// </summary>
        public static Car GetPreCar(Car meCar, List<Car> cars)
        {
            return cars
                .Where(c => c.CarIndex != meCar.CarIndex) // Exclude self
                 .Where(c => !c.InPits)
                .Where(c => IsCarAhead(meCar.SplinePosition, c.SplinePosition)) // Ahead on track
                .OrderBy(c => AdjustedSplinePosition(meCar.SplinePosition, c.SplinePosition)) // Closest one ahead
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the car positioned behind the given car on the track.
        /// </summary>
        public static Car GetPostCar(Car meCar, List<Car> cars)
        {
            return cars
                .Where(c => c.CarIndex != meCar.CarIndex) // Exclude self
                .Where(c => !c.InPits) 
                .Where(c => IsCarBehind(meCar.SplinePosition, c.SplinePosition)) // Behind on track
                .OrderByDescending(c => AdjustedSplinePosition(meCar.SplinePosition, c.SplinePosition)) // Closest one behind
                .FirstOrDefault();
        }

        /// <summary>
        /// Determines if a car is ahead, considering track wraparound.
        /// </summary>
        private static bool IsCarAhead(float myPos, float carPos)
        {
            float diff = carPos - myPos;
            return diff > 0 && diff < (TrackLength * 0.5f);
        }

        /// <summary>
        /// Determines if a car is behind, considering track wraparound.
        /// </summary>
        private static bool IsCarBehind(float myPos, float carPos)
        {
            float diff = carPos - myPos;
            return diff < 0 || diff > (TrackLength * 0.5f);
        }

        /// <summary>
        /// Adjusts the spline position for proper ordering, considering wraparound.
        /// </summary>
        private static float AdjustedSplinePosition(float myPos, float carPos)
        {
            return carPos < myPos ? carPos + TrackLength : carPos;
        }
    }
}
