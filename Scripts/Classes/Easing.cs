
namespace unexpected
{
    /// <summary>This class is used to manage easing functions</summary>
    public class Easing
    {

        /// <summary>Ease in and out in a desired timespan</summary>
        /// <param name="currentTime">is the current state of easing (between 0 and 1)</param>
        /// <returns>a value between 0 and 1 of the translated state of easing</returns>
        public static float EaseInOut(float currentTime)
        {
            currentTime *= 2.0f;
            if (currentTime < 1)
                return .5f * currentTime * currentTime * currentTime;

            currentTime -= 2;
            return .5f * (currentTime * currentTime * currentTime + 2.0f);
        }

        /// <summary>Ease in in a desired timespan</summary>
        /// <param name="currentTime">is the current state of easing (between 0 and 1)</param>
        /// <returns>a value between 0 and 1 of the translated state of easing</returns>
        public static float EaseIn(float currentTime)
        {
            return currentTime * currentTime * currentTime;
        }

        /// <summary>Ease out in a desired timespan</summary>
        /// <param name="currentTime">is the current state of easing (between 0 and 1)</param>
        /// <returns>a value between 0 and 1 of the translated state of easing</returns>
        public static float EaseOut(float currentTime)
        {
            currentTime -= 1;
            return currentTime * currentTime * currentTime + 1.0f;
        }
    }
}