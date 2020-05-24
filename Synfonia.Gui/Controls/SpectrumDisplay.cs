﻿using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using ReactiveUI;

namespace Synfonia.Controls
{
    public class SpectrumDisplay : UserControl
    {
        public static readonly DirectProperty<SpectrumDisplay, double[,]> FFTDataProperty =
            AvaloniaProperty.RegisterDirect<SpectrumDisplay, double[,]>(
                nameof(FFTData),
                o => o.FFTData,
                (o, v) => o.FFTData = v);

        private double[,] _averagedData;
        private readonly int _averageLevel = 5;
        private bool _center = true;

        private double[,] _fftData;
        private double _lastStrokeThickness;
        private IPen _linePen;

        static SpectrumDisplay()
        {
            AffectsMeasure<SpectrumDisplay>(FFTDataProperty);
        }

        public SpectrumDisplay()
        {
            this.GetObservable(FFTDataProperty)
                .Throttle(TimeSpan.FromMilliseconds(250))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    if (FFTData != null) FFTData = new double[FFTData.GetLength(0), FFTData.GetLength(1)];
                });
        }

        public double[,] FFTData
        {
            get => _fftData;
            set => SetAndRaise(FFTDataProperty, ref _fftData, value);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (FFTData != null)
            {
                if (_averagedData is null || FFTData.GetLength(1) != _averagedData.GetLength(1))
                    _averagedData = new double[2, FFTData.GetLength(1)];

                for (var channel = 0; channel < 2; channel++)
                for (var i = 0; i < FFTData.GetLength(1); i++)
                {
                    _averagedData[channel, i] -= _averagedData[channel, i] / _averageLevel;
                    _averagedData[channel, i] += Math.Abs(FFTData[channel, i]) / _averageLevel;
                }

                var length = FFTData.GetLength(1);
                var gaps = length * 2 + 1;

                var gapSize = 1.0;
                //if ((gaps * gapSize) > Bounds.Width)
                {
                    gapSize = 0.25;
                }

                var binStroke = (Bounds.Width - gaps * gapSize) / (length * 2);

                if (_lastStrokeThickness != binStroke)
                {
                    _lastStrokeThickness = binStroke;
                    _linePen = new Pen(new SolidColorBrush(Colors.Gray, 0.5), _lastStrokeThickness);
                }

                var x = binStroke / 2 + gapSize;

                if (_center)
                    for (var channel = 0; channel < 2; channel++)
                    for (var i = 0; i < length; i++)
                    {
                        var value = Bounds.Height / 2 * _averagedData[channel, channel == 0 ? length - 1 - i : i];
                        var center = Bounds.Height / 2;

                        context.DrawLine(_linePen, new Point(x, center - value), new Point(x, center + value));
                        x += binStroke + gapSize;
                    }
                else
                    for (var channel = 0; channel < 2; channel++)
                    for (var i = 0; i < length; i++)
                    {
                        context.DrawLine(_linePen, new Point(x, Bounds.Height),
                            new Point(x,
                                Bounds.Height * (1 - _averagedData[channel, channel == 0 ? length - 1 - i : i])));
                        x += binStroke + gapSize;
                    }

                Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            _center = !_center;
        }
    }
}