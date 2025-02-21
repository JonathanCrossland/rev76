using Rev76.DataModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Rev76.Windows.Helpers
{
    public static class SplineUtil
    {
        private const float TrackLength = 1.0f; // Normalized track range (0.0 to 1.0)

        public static Car GetPreCar(Car meCar, ConcurrentBag<Car> cars, float trackLength)
        {
          
            var validCars = cars
                .Where(c => c.CarIndex != meCar.CarIndex) // Exclude self
                .Where(c => !c.InPits) // Ignore cars in pits
                .ToList();

            if (validCars.Count == 0) return null; // No other cars on track
            if (validCars.Count == 1) // Only one other car, use SplinePosition
            {
                return validCars.First().SplinePosition > meCar.SplinePosition ? validCars.First() : null;
            }

            // Sort cars by LapNumber first, then by SplinePosition
            var sortedCars = validCars
                .OrderByDescending(c => c.Laps)
                .ThenBy(c => c.SplinePosition)
                .ToList();

            // Find the closest car ahead, considering wrap-around
            Car nextCar = null;
            float minDistance = float.MaxValue;

            foreach (var car in sortedCars)
            {
                float distance = (car.SplinePosition - meCar.SplinePosition + trackLength) % trackLength;
                if (distance > 0 && distance < minDistance)
                {
                    minDistance = distance;
                    nextCar = car;
                }
            }

            return nextCar;
        }

        public static Car GetPostCar(Car meCar, ConcurrentBag<Car> cars, float trackLength)
        {
            var validCars = cars
                .Where(c => c.CarIndex != meCar.CarIndex) // Exclude self
                .Where(c => !c.InPits) // Ignore cars in pits
                .ToList();

            if (validCars.Count == 0) return null; // No other cars on track
            if (validCars.Count == 1) // Only one other car, use SplinePosition
            {
                return validCars.First().SplinePosition < meCar.SplinePosition ? validCars.First() : null;
            }

            // Sort cars by LapNumber first, then by SplinePosition
            var sortedCars = validCars
                .OrderByDescending(c => c.Laps)
                .ThenBy(c => c.SplinePosition)
                .ToList();

            // Find the closest car behind, considering wrap-around
            Car previousCar = null;
            float minDistance = float.MaxValue;

            foreach (var car in sortedCars)
            {
                float distance = (meCar.SplinePosition - car.SplinePosition + trackLength) % trackLength;
                if (distance > 0 && distance < minDistance)
                {
                    minDistance = distance;
                    previousCar = car;
                }
            }

            return previousCar;
        }




        /// <summary>
        /// Computes absolute distance between two spline positions, handling wraparound naturally.
        /// </summary>
        private static float GetAbsoluteDistance(float myPos, float carPos)
        {
            float directDist = Math.Abs(carPos - myPos);
            float wrapDist = 1.0f - directDist; // Distance when crossing 0

            return Math.Min(directDist, wrapDist); // Take the shortest possible distance
        }


    }
}
