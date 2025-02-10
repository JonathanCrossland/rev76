using Rev76.DataModels;
using System.Collections.Generic;
using System.Linq;

namespace Rev76.Windows.Helpers
{
    public static class SplineUtil
    {
        private const float TrackLength = 1.0f; // Normalized track range (0.0 to 1.0)

        /// <summary>
        /// Gets the car positioned ahead of the given car on the track.
        /// </summary>
        public static Car GetPreCar(Car meCar, List<Car> cars)
        {
            return cars
                .Where(c => c.CarIndex != meCar.CarIndex) // Exclude self
                .Where(c => !c.InPits)
                .Where(c => IsCarAhead(meCar.SplinePosition, c.SplinePosition)) // Ahead on track
                .OrderBy(c => GetSplineDistance(meCar.SplinePosition, c.SplinePosition)) // Closest one ahead
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
                .OrderByDescending(c => GetSplineDistance(meCar.SplinePosition, c.SplinePosition)) // Closest one behind
                .FirstOrDefault();
        }

        /// <summary>
        /// Determines if a car is ahead, considering track wraparound.
        /// </summary>
        private static bool IsCarAhead(float myPos, float carPos)
        {
            float diff = GetSplineDistance(myPos, carPos);
            return diff > 0;
        }

        /// <summary>
        /// Determines if a car is behind, considering track wraparound.
        /// </summary>
        private static bool IsCarBehind(float myPos, float carPos)
        {
            float diff = GetSplineDistance(myPos, carPos);
            return diff < 0;
        }

        /// <summary>
        /// Computes the correct spline distance, considering track wraparound.
        /// </summary>
        private static float GetSplineDistance(float myPos, float carPos)
        {
            float diff = carPos - myPos;

            // Correct wrap-around: Ensures closest distance is always chosen
            if (diff > 0.5f)
                diff -= 1.0f;
            else if (diff < -0.5f)
                diff += 1.0f;

            return diff;
        }
    }
}
