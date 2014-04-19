using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Controls;
namespace MusicBucket
{
    public class GridWidthAnimation : AnimationTimeline
    {

        double _sharpness,_bouncyness;
        int _frequency;
        GridLength _from, _to;

        public GridWidthAnimation()
        {
            this.FillBehavior = FillBehavior.Stop;
        }

        public GridLength From
        {
            set 
            {
                _from = value;
            }
            get 
            {
                return _from;
            }
        }

        public GridLength To
        {
            set 
            {
                _to = value;
            }
            get 
            {
                return _to;
            }
        }

        public double Sharpness
        {
            get
            {
                return _sharpness;
            }
            set 
            {
                if (value >= 0 && value < 1)
                {
                    _sharpness = value;
                }
                else
                {
                    throw new MusicBucketException(String.Format(Properties.Resources.SharpnessValueOutOfRange, value));
                }
            }
        }


        public double Bouncyness
        {
            get 
            {
                return _bouncyness;
            }
            set
            {
                if (value >= 0 && value <= 0.5)
                {
                    _bouncyness = value;
                }
                else
                {
                    throw new MusicBucketException(String.Format(Properties.Resources.BouncynessValueOutOfRange, value));
                }
            }
        }

        public int Frequency
        {
            get
            {
                return _frequency;
            }
            set
            {
                if (value > 0 && value < 2134)
                {
                    _frequency = value;
                }
                else
                {
                    throw new MusicBucketException(Properties.Resources.FrequencyOutOfRange);
                }
            }
        }


        protected override System.Windows.Freezable CreateInstanceCore()
        {
            return this;
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            GridLength clength;
            double theval;
            if (animationClock.CurrentProgress.HasValue)
            {
                theval = BounceFunction2(animationClock.CurrentProgress.Value, this.From.Value, this.To.Value, this.Sharpness, this.Frequency, this.Bouncyness);
                clength = new GridLength(theval); //new GridLength(animationClock.CurrentProgress.Value * (this.To.Value - this.From.Value) + this.From.Value);
            }
            else
            {
                clength = new GridLength(this.To.Value);
            }
            return clength;
        }

        public override bool IsDestinationDefault
        {
            get
            {
                return false;
            }
        }

        public override Type TargetPropertyType
        {
            get { return typeof(GridLength); }
        }

        /// <summary>
        /// Implementation of a bouncing function for the animation of the grid lengths.
        /// </summary>
        /// <param name="time">actual time value, must be between 0 and 1</param>
        /// <param name="startval">the value of the function at t=0</param>
        /// <param name="endval">the value of the function at t=1</param>
        /// <param name="sharpness">a sharpness factor, ranges from 0 to 1 (1 itself not included), 0 is smooth, (almost) 1 is sharp</param>
        /// <param name="frequency">the number of oscillation whitin the transition, must range from 1 up to maybe 1090 ... or 2134</param>
        /// <param name="bouncefactor">the amount of bouncyness, goes from 0 to 0.5</param>
        /// <returns></returns>
        public double BounceFunction(double time,double startval,double endval,double sharpness,int frequency,double bouncefactor)
        {
            double res = 0;
            double tk, tbar;
            tk = 0.5 * sharpness;
            if (time < 0.5 - tk)
            {
                if (startval > endval)
                {
                    res = startval - 2 * time * Math.Abs(Math.Sin(frequency * 2.0 * Math.PI * time)) * Math.Abs(startval-endval) * bouncefactor;
                }
                else
                {
                    res = startval + 2 * time * Math.Abs(Math.Sin(frequency * 2.0 * Math.PI * time)) * Math.Abs(startval - endval) * bouncefactor;
                }
            }
            else if (time >= 0.5 - tk && time < 0.5)
            {
                tbar = (time - 0.5 + tk) / (2.0*tk);
                if (startval > endval)
                {
                    res = (endval-startval) * (3.0 * tbar * tbar - 2.0 * tbar * tbar * tbar) - 2.0 * time * Math.Abs(Math.Sin(frequency * 2.0 * Math.PI * time)) * Math.Abs(startval - endval) * bouncefactor + startval;
                }
                else
                {
                    res = (endval - startval) * (3.0 * tbar * tbar - 2.0 * tbar * tbar * tbar) + 2.0 * time * Math.Abs(Math.Sin(frequency * 2.0 * Math.PI * time)) * Math.Abs(startval - endval) * bouncefactor + startval;
                }
            }
            else if (time >= 0.5 && time < 0.5 + tk)
            {
                tbar = (time - 0.5 + tk) / (2.0*tk);
                if (startval > endval)
                {
                    res = (endval - startval) * (3.0 * tbar * tbar - 2.0 * tbar * tbar * tbar) + 2.0 * (1.0 - time) * Math.Abs(Math.Sin(frequency * 2.0 * Math.PI * time)) * Math.Abs(startval - endval) * bouncefactor + startval;
                }
                else
                {
                    res = (endval - startval) * (3.0 * tbar * tbar - 2.0 * tbar * tbar * tbar) - 2.0 * (1.0 - time) * Math.Abs(Math.Sin(frequency * 2.0 * Math.PI * time)) * Math.Abs(startval - endval) * bouncefactor + startval;
                }
            }
            else
            {
                if (startval > endval)
                {
                    res = endval + 2 * (1.0- time) * Math.Abs(Math.Sin(frequency * 2.0 * Math.PI * time)) * Math.Abs(startval - endval) * bouncefactor;
                }
                else
                {
                    res = endval - 2 * (1.0 - time) * Math.Abs(Math.Sin(frequency * 2.0 * Math.PI * time)) * Math.Abs(startval - endval) * bouncefactor;
                }
            }
            return res;
        }

        public double BounceFunction2(double time,double startval,double endval,double steepness,int frequency,double rebouncefactor)
        {
            double res = 0;
            double tbar;
            if (time < steepness)
            {
                tbar = time / steepness;
                res = startval + (endval - startval) * (3.0 * tbar * tbar - 2.0 * tbar * tbar * tbar);
            }
            else
            {
                tbar=(time-steepness)/(1.0-steepness);
                if(endval<startval)
                {
                    res = Math.Abs(Math.Sin(2.0 * Math.PI * frequency * tbar)) * Math.Abs(endval - startval) * rebouncefactor * (1.0 - tbar) + endval;
                }
                else
                {
                    res = - Math.Abs(Math.Sin(2.0 * Math.PI * frequency * tbar)) * Math.Abs(endval - startval) * rebouncefactor * (1.0 - tbar) + endval;
                }

            }
            return res;
        }
    }
}
