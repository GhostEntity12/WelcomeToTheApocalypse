using System.Collections;
using TMPro;
using UnityEngine;

using static Ghost.Callbacks;

namespace Ghost
{
    /// <summary>
    /// Class for fading elements.
    /// Uses Callbacks.cs
    /// </summary>
    public static class Fade
    {
        /// <summary>
        /// Fades a TextMeshPro element to a given transparency
        /// </summary>
        /// <param name="tmp">The element to fade</param>
        /// <param name="lerpTime">How long it should take to fade</param>
        /// <param name="start">The starting transparency</param>
        /// <param name="end">The ending transparency</param>
        /// <param name="delay">The time to wait before fading</param>
        /// <returns></returns>
        public static IEnumerator FadeTextElement(TextMeshPro tmp, float lerpTime, float start, float end, float delay = 0f)
        {
            // Wait for delay
            yield return new WaitForSeconds(delay);

            //Setting variables for tracking time
            float timeAtStart = Time.time;
            float timeSinceStart;
            float percentageComplete = 0;

            while (percentageComplete < 1) // Keeps looping until the lerp is complete
            {
                timeSinceStart = Time.time - timeAtStart;
                percentageComplete = timeSinceStart / lerpTime;

                float currentValue = Mathf.Lerp(start, end, percentageComplete);


                tmp.alpha = currentValue;
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Fades a CanvasGroup to a given transparency
        /// </summary>
        /// <param name="cg">The element to fade</param>
        /// <param name="lerpTime">How long it should take to fade</param>
        /// <param name="start">The starting transparency</param>
        /// <param name="end">The ending transparency</param>
        /// <param name="delay">The time to wait before fading</param>
        /// <returns></returns>
        public static IEnumerator FadeCanvasGroup(CanvasGroup cg, float lerpTime, float start, float end, float delay = 0f)
        {
            // Wait for delay
            yield return new WaitForSeconds(delay);

            // Setting variables for tracking time
            float timeAtStart = Time.time;
            float timeSinceStart;
            float percentageComplete = 0;

            while (percentageComplete < 1) // Keeps looping until the lerp is complete
            {
                // Get new time
                timeSinceStart = Time.time - timeAtStart;
                percentageComplete = timeSinceStart / lerpTime;

                // Get lerp value
                float currentValue = Mathf.Lerp(start, end, percentageComplete);

                // Setting the transparency
                cg.alpha = currentValue;
                yield return new WaitForEndOfFrame();
            }
        }

        #region Fading CanvasGroups with a callback
        /// <summary>
        /// Fades a CanvasGroup to a given transparency and runs a function on completion
        /// </summary>
        /// <param name="cg">The element to fade</param>
        /// <param name="lerpTime">How long it should take to fade</param>
        /// <param name="start">The starting transparency</param>
        /// <param name="end">The ending transparency</param>
        /// <param name="callback">The function to call once the fade is complete</param>
        /// <param name="delay">The time to wait before fading</param>
        /// <returns></returns>
        public static IEnumerator FadeCanvasGroup(CanvasGroup cg, float lerpTime, float start, float end, CallbackDelegateNull callback, float delay = 0f)
        {
            yield return FadeCanvasGroup(cg: cg, lerpTime: lerpTime, start: start, end: end, delay: delay);

            callback();
        }

        /// <summary>
        /// Fades a CanvasGroup to a given transparency and runs a function which takes an int on completion
        /// </summary>
        /// <param name="cg">The element to fade</param>
        /// <param name="lerpTime">How long it should take to fade</param>
        /// <param name="start">The starting transparency</param>
        /// <param name="end">The ending transparency</param>
        /// <param name="callback">The function to call once the fade is complete</param>
        /// <param name="callbackInt">The int to be passed as a parameter into the callback</param>
        /// <param name="delay">The time to wait before fading</param>
        /// <returns></returns>
        public static IEnumerator FadeCanvasGroup(CanvasGroup cg, float lerpTime, float start, float end, CallbackDelegateInt callback, int callbackInt, float delay = 0f)
        {
            yield return FadeCanvasGroup(cg: cg, lerpTime: lerpTime, start: start, end: end, delay: delay);

            callback(callbackInt);
        }

        /// <summary>
        /// Fades a CanvasGroup to a given transparency and runs a function which takes a string on completion
        /// </summary>
        /// <param name="cg">The element to fade</param>
        /// <param name="lerpTime">How long it should take to fade</param>
        /// <param name="start">The starting transparency</param>
        /// <param name="end">The ending transparency</param>
        /// <param name="callback">The function to call once the fade is complete</param>
        /// <param name="callbackString">The string to be passed as a parameter into the callback</param>
        /// <param name="delay">The time to wait before fading</param>
        /// <returns></returns>
        public static IEnumerator FadeCanvasGroup(CanvasGroup cg, float lerpTime, float start, float end, CallbackDelegateString callback, string callbackString, float delay = 0f)
        {
            yield return FadeCanvasGroup(cg: cg, lerpTime: lerpTime, start: start, end: end, delay: delay);

            callback(callbackString);
        }
        #endregion

        /// <summary>
        /// Fades a Material to a given transparency. Requires that the material is not of type opaque
        /// </summary>
        /// <param name="m">The element to fade</param>
        /// <param name="lerpTime">How long it should take to fade</param>
        /// <param name="start">The starting transparency</param>
        /// <param name="end">The ending transparency</param>
        /// <param name="delay">The time to wait before fading</param>
        /// <returns></returns>
        public static IEnumerator FadeMaterial(Material m, float lerpTime, float start, float end, float delay = 0f)
        {
            // Wait for delay
            yield return new WaitForSeconds(delay);

            // Setting variables for tracking time
            float timeAtStart = Time.time;
            float timeSinceStart;
            float percentageComplete = 0;

            while (percentageComplete < 1) // Keeps looping until the lerp is complete
            {
                // Get new time
                timeSinceStart = Time.time - timeAtStart;
                percentageComplete = timeSinceStart / lerpTime;

                // Get lerp value
                float currentValue = Mathf.Lerp(start, end, percentageComplete);

                // Setting the transparency
                m.color = new Color(m.color.r, m.color.g, m.color.b, currentValue);
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Fades a AudioSource's volume to a given volume
        /// </summary>
        /// <param name="audio">The element to fade</param>
        /// <param name="lerpTime">How long it should take to fade</param>
        /// <param name="start">The starting transparency</param>
        /// <param name="end">The ending transparency</param>
        /// <param name="delay">The time to wait before fading</param>
        /// <returns></returns>
        public static IEnumerator FadeAudio(AudioSource audio, float lerpTime, float start, float end, float delay = 0f)
        {
            // Wait for delay
            yield return new WaitForSeconds(delay);

            // Setting variables for tracking time
            float timeAtStart = Time.time;
            float timeSinceStart;
            float percentageComplete = 0;

            while (percentageComplete < 1) // Keeps looping until the lerp is complete
            {
                // Get new time
                timeSinceStart = Time.time - timeAtStart;
                percentageComplete = timeSinceStart / lerpTime;

                // Get lerp value
                float currentValue = Mathf.Lerp(start, end, percentageComplete);

                // Setting the volume
                audio.volume = currentValue;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
