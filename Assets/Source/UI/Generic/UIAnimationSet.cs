using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

sealed internal class UIAnimationSet : MonoBehaviour
{
    #region Values
    [Serializable]
    public class Animation
    {
        /// <summary>
        /// Determines if the animation is currently playing.
        /// </summary>
        internal bool playing;

        /// <summary>
        /// Defines the name of the animation.
        /// </summary>
        [SerializeField] internal string name;

        /// <summary>
        /// Determines if the animation is to play upon game object enable.
        /// </summary>
        [Space(15)]
        [SerializeField]
        internal bool playOnEnable;
        /// <summary>
        /// Determines if the animation is to repeat upon completion.
        /// </summary>
        [SerializeField] internal bool loop;

        /// <summary>
        /// Determines if the animation is slave to Unity's Time.timeScale
        /// </summary>
        [Space(15)]
        [SerializeField]
        internal bool timeScaleIndependent;

        /// <summary>
        /// Defines the duration in which the animation will wait to begin.
        /// </summary>
        [Space(15)]
        [SerializeField]
        internal float startDelay;
        /// <summary>
        /// Defines the progress along the start delay.
        /// </summary>
        internal float startTime;
        /// <summary>
        /// Defines the time span of the animation.
        /// </summary>
        [SerializeField] internal float duration;
        /// <summary>
        /// Defines the progress along the duration of the animation.
        /// </summary>
        internal float progress;
        

        /// <summary>
        /// Defines the action that is to be invoked upon animation completion.
        /// </summary>
        [Space(10)]
        [HideInInspector]
        public Action action;

        public enum AnimationCompletionAction
        {
            None,
            Deactivate,
            Destroy
        }
        /// <summary>
        /// Defines a specific action that is to trigger upon completion.
        /// </summary>
        [SerializeField] internal AnimationCompletionAction completionAction;

        [Serializable]
        public class AnimationColor
        {
            /// <summary>
            /// Determines if the animation is to use color.
            /// </summary>
            [SerializeField] internal bool use;
            /// <summary>
            /// Defines the color of the maskable graphic along the span of the animation.
            /// </summary>
            [SerializeField] internal Gradient color;
        }
        [Space(15)]
        [SerializeField]
        internal AnimationColor color;

        [Serializable]
        public class AnimationPosition
        {
            /// <summary>
            /// Determines if the animation is to use position.
            /// </summary>
            [SerializeField] internal bool use;
            /// <summary>
            /// Defines the target position that the rect transform is to progress to over the span of the animation.
            /// </summary>
            [SerializeField] internal Vector3 position;

            [Serializable]
            public class RectTransfromUtilityAnchor
            {
                public Vector2 min;
                public Vector2 max;
            }
            /// <summary>
            /// Defines the target anchor that the rect transform is to progress to over the span of the animation.
            /// </summary>
            [SerializeField] internal RectTransfromUtilityAnchor anchor;
        }
        [Space(15)]
        public AnimationPosition position;


        [Serializable]
        public class AnimationRotation
        {
            /// <summary>
            /// Determines if the animation is to use rotation.
            /// </summary>
            [SerializeField] internal bool use;
            /// <summary>
            /// Determines the target rotation that the rect transform is to progress to over the duration of the animation.
            /// </summary>
            [SerializeField] internal Vector3 rotation;
        }
        [Space(15)]
        public AnimationRotation rotation;

        [Serializable]
        public class AnimationScale
        {
            /// <summary>
            /// Determines if the animation is to use scale.
            /// </summary>
            [SerializeField] internal bool use;
            /// <summary>
            /// Determines if the scale is to be set to zero along the duration of the start delay.
            /// </summary>
            [SerializeField] internal bool startAtZeroOnDelay;
            /// <summary>
            /// Determines if the X axis is to be multiplied by -1.
            /// </summary>
            [SerializeField] internal bool flipX;
            /// <summary>
            /// Determines the X axis' scale of the rect transform over the progression of the animation.
            /// </summary>
            [SerializeField] internal AnimationCurve scaleX;
            /// <summary>
            /// Determines if the Y axis is to be multiplied by -1.
            /// </summary>
            [SerializeField] internal bool flipY;
            /// <summary>
            /// Determines the Y axis' scale of the rect transform over the progression of the animation.
            /// </summary>
            [SerializeField] internal AnimationCurve scaleY;
            /// <summary>
            /// Determines if the Z axis is to be multiplied by -1.
            /// </summary>
            [SerializeField] internal bool flipZ;
            /// <summary>
            /// Determines the Z axis' scale of the rect transform over the progression of the animation.
            /// </summary>
            [SerializeField] internal AnimationCurve scaleZ;
        }
        [Space(15)]
        [SerializeField]
        internal AnimationScale Scale;
    }
    [SerializeField] internal Animation[] Animations;

    // Defines the rect transform component of the game object.
    private RectTransform _rectTransform;
    // Defines the maskable graphic component of the game object.
    private MaskableGraphic _maskableGraphic;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        PlayOnEnable();
    }
    
    private void LateUpdate()
    {
        StepAnimations();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        _rectTransform = this.GetComponent<RectTransform>();
        _maskableGraphic = this.transform.GetComponent<MaskableGraphic>();
    }

    public void PlayOnEnable()
    {
        // Find each animation that is set to play on enable and play it.
        foreach (Animation animation in Animations)
            if (animation.playOnEnable)
            {
                StopAll();
                Play(animation.name, true);
            }
    }

    private void StepAnimations()
    {
        // Processes each animation.
        foreach (Animation animation in Animations)
        {
            if (!animation.playing || animation.duration == 0 || (animation.startTime > Time.time))
                continue;

            // Define the progress of the animation
            animation.progress = Mathf.Min(animation.progress + (animation.timeScaleIndependent ? Time.unscaledDeltaTime : Time.deltaTime), animation.duration);
            float time = animation.progress / animation.duration;

            // Step each aspect
            StepPosition(animation, time);
            StepRotation(animation, time);
            StepScale(animation, time);
            StepColor(animation, time);

            if (animation.progress == animation.duration)
            {
                // If the animation has reached its end, process actions, reset progress and loop if contextual
                ProcessActions(animation);

                animation.progress = 0;
                if (!animation.loop)
                    animation.playing = false;
            }
        }
    }
    
    private void StepPosition(Animation animation, float time)
    {
        // Progresses the position of the animation.
        if (!animation.position.use)
            return;

        _rectTransform.anchorMin = Vector2.Lerp(_rectTransform.anchorMin, animation.position.anchor.min, time);
        _rectTransform.anchorMax = Vector2.Lerp(_rectTransform.anchorMax, animation.position.anchor.max, time);
        _rectTransform.anchoredPosition = Vector2.Lerp(_rectTransform.anchoredPosition, animation.position.position, time);
    }
    private void StepRotation(Animation animation, float time)
    {
        // Progresses the rotation of the animation.
        if (animation.rotation.use)
            _rectTransform.eulerAngles = animation.rotation.rotation * time;
    }
    private void StepScale(Animation animation, float time)
    {
        // Progresses the scale of the animation.
        if (!animation.Scale.use)
            return;

        Vector3 scale = Vector3.zero;
        scale.x = animation.Scale.scaleX.Evaluate(time) * (animation.Scale.flipX ? -1 : 1);
        scale.y = animation.Scale.scaleY.Evaluate(time) * (animation.Scale.flipY ? -1 : 1);
        scale.z = animation.Scale.scaleZ.Evaluate(time) * (animation.Scale.flipZ ? -1 : 1);

        _rectTransform.localScale = scale;
    }
    private void StepColor(Animation animation, float time)
    {
        // Progresses the color of the animation.
        if (animation.color.use)
            _maskableGraphic.color = animation.color.color.Evaluate(time);
    }

    private void ProcessActions(Animation animation)
    {
        // Processes the defined actions upon completion of the animation.
        if (animation.action != null)
            animation.action.Invoke();

        switch (animation.completionAction)
        {
            case Animation.AnimationCompletionAction.Deactivate:
                this.gameObject.SetActive(false);
                break;
            case Animation.AnimationCompletionAction.Destroy:
                Destroy(this.gameObject);
                break;
        }
    }

    public void Play(string name)
    {
        Play(name, true);
    }

    /// <summary>
    /// Plays a specified animation within the set.
    /// </summary>
    /// <param name="name"> The name of the animation that is to be played.</param>
    /// <param name="completionCallback"> The action that is to be called upon the completion of the animation.</param>
    public void Play(string name, bool reset = true, Action completionCallback = null)
    {
        Animation animation = Array.Find(Animations, p => p.name == name);
        if (animation == null)
            return;

        if (!reset && animation.playing)
            return;

        animation.playing = true;
        animation.startTime = Time.time + animation.startDelay;
        animation.progress = 0;
        animation.action = completionCallback;
    }

    /// <summary>
    /// Pauses a specified animation within the set.
    /// </summary>
    /// <param name="name"> The name of the animation that is to be paused.</param>
    public void Pause(string name)
    {
        Animation animation = Array.Find(Animations, p => p.name == name);
        animation.playing = false;
    }
    /// <summary>
    /// Pauses each animation within the set.
    /// </summary>
    public void PauseAll()
    {
        foreach (Animation animation in Animations)
            animation.playing = false;
    }

    /// <summary>
    /// Stops a specified animation within the set.
    /// </summary>
    /// <param name="name"> The name of the animation that is to be stopped.</param>
    public void Stop(string name)
    {
        Animation animation = Array.Find(Animations, p => p.name == name);
        animation.playing = false;
        animation.progress = 0;
    }
    /// <summary>
    /// Stops each animation within the set.
    /// </summary>
    public void StopAll()
    {
        foreach (Animation animation in Animations)
        {
            animation.playing = false;
            animation.progress = 0;
        }
    }
    #endregion
}
