﻿using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Input;

namespace Synfonia.Controls
{
    public class VolumeSlider : RangeBase
    { 
        // Slider required parts
        private Track _track;
        private SeekTrackButton _decreaseButton;
        private SeekTrackButton _increaseButton;

        /// <summary>
        /// Initializes static members of the <see cref="VolumeSlider"/> class. 
        /// </summary>
        static VolumeSlider()
        {
            Thumb.DragStartedEvent.AddClassHandler<VolumeSlider>((x, e) => x.OnThumbDragStarted(e), RoutingStrategies.Bubble);
            Thumb.DragDeltaEvent.AddClassHandler<VolumeSlider>((x, e) => x.OnThumbDragDelta(e), RoutingStrategies.Bubble);
            Thumb.DragCompletedEvent.AddClassHandler<VolumeSlider>((x, e) => x.OnThumbDragCompleted(e), RoutingStrategies.Bubble);
        }

        /// <summary>
        /// Instantiates a new instance of the <see cref="VolumeSlider"/> class. 
        /// </summary>
        public VolumeSlider()
        {
        }

        /// <inheritdoc/>
        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        { 
            _decreaseButton = e.NameScope.Find<SeekTrackButton>("PART_DecreaseButton");
            _track = e.NameScope.Find<Track>("PART_Track");
            _increaseButton = e.NameScope.Find<SeekTrackButton>("PART_IncreaseButton");

            if (_decreaseButton != null)
            {
                _decreaseButton.PointerPressed += DC_PP;
            }

            if (_increaseButton != null)
            {
                _increaseButton.PointerPressed += IC_PP;
            }
        }

        private void IC_PP(object sender, PointerPressedEventArgs e)
        {
            var x = e.GetCurrentPoint(_track);
            Value = x.Position.X / _track.Bounds.Width;
        }

        private void DC_PP(object sender, PointerPressedEventArgs e)
        {
            var x = e.GetCurrentPoint(_track);
            Value = x.Position.X / _track.Bounds.Width;
        }

        /// <summary>
        /// Called when user start dragging the <see cref="Thumb"/>.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnThumbDragStarted(VectorEventArgs e)
        {
            
        }

        /// <summary>
        /// Called when user dragging the <see cref="Thumb"/>.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnThumbDragDelta(VectorEventArgs e)
        {
            if (e.Source is Thumb thumb && _track?.Thumb == thumb)
            {
                MoveToNextTick(_track.Value);
            }
        }

        /// <summary>
        /// Called when user stop dragging the <see cref="Thumb"/>.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnThumbDragCompleted(VectorEventArgs e)
        {
            
        }

        /// <summary>
        /// Searches for the closest tick and sets Value to that tick.
        /// </summary>
        /// <param name="value">Value that want to snap to closest Tick.</param>
        private void MoveToNextTick(double value)
        {
            Value = Math.Max(Minimum, Math.Min(Maximum, value));
        }
    }
}
