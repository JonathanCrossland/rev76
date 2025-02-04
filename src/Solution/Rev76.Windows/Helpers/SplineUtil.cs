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
            if (meCar == null) return null;
            return cars
                .Where(c => c.CarIndex != meCar.CarIndex && !c.InPits)
                .Where(c => GlobalDiff(meCar.Laps, meCar.SplinePosition, c.Laps, c.SplinePosition) > 0)
                .OrderBy(c => GlobalDiff(meCar.Laps, meCar.SplinePosition, c.Laps, c.SplinePosition))
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the car positioned behind the given car on the track.
        /// </summary>
        public static Car GetPostCar(Car meCar, List<Car> cars)
        {
            if (meCar == null) return null;
            return cars
                .Where(c => c.CarIndex != meCar.CarIndex && !c.InPits)
                .Where(c => GlobalDiff(meCar.Laps, meCar.SplinePosition, c.Laps, c.SplinePosition) < 0)
                .OrderByDescending(c => GlobalDiff(meCar.Laps, meCar.SplinePosition, c.Laps, c.SplinePosition))
                .FirstOrDefault();
        }

        private static float GlobalDiff(int myLap, float myPos, int carLap, float carPos)
        {
            float trackLength = GameData.Track.TrackLength;
            // Compute global positions
            float myGlobal = myLap * trackLength + myPos;
            float carGlobal = carLap * trackLength + carPos;
            float diff = carGlobal - myGlobal;

            // Only adjust for wrap-around if on the same lap
            if (carLap == myLap)
            {
                if (diff < -trackLength / 2f)
                    diff += trackLength;
                else if (diff > trackLength / 2f)
                    diff -= trackLength;
            }
            return diff;
        }

        /// <summary>
        /// Determines if a car is ahead, considering track wraparound.
        /// </summary>
        private static bool IsCarAhead(int myLap, float myPos, int carLap, float carPos, float trackLength)
        {
            if (carLap > myLap)
                return true;

            if (carLap == myLap)
            {
                float diff = carPos - myPos;

                if (diff < -trackLength * 0.5f)
                    diff += trackLength;
                else if (diff > trackLength * 0.5f)
                    diff -= trackLength;

                return diff > 0;
            }

            return false;
        }

        /// <summary>
        /// Determines if a car is behind, considering track wraparound.
        /// </summary>
        private static bool IsCarBehind(int myLap, float myPos, int carLap, float carPos, float trackLength)
        {
            if (carLap < myLap)
                return true;

            if (carLap == myLap)
            {
                float diff = carPos - myPos;

                if (diff < -trackLength * 0.5f)
                    diff += trackLength;
                else if (diff > trackLength * 0.5f)
                    diff -= trackLength;

                return diff < 0;
            }

            return false;
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
