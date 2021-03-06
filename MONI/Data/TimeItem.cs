﻿using System;
using System.Linq;
using Newtonsoft.Json;

namespace MONI.Data
{
    public class TimeItem : IComparable<TimeItem>
    {
        public TimeItem(int hour) : this(hour, 0)
        {
        }

        [JsonConstructor]
        public TimeItem(int hour, int minute)
        {
            if (hour > 24)
            {
                throw new ArgumentOutOfRangeException("hour", "darf nicht größer als 24 sein");
            }
            if (hour < 0)
            {
                throw new ArgumentOutOfRangeException("hour", "darf nicht kleiner als 0 sein");
            }
            if (minute > 60)
            {
                throw new ArgumentOutOfRangeException("minute", "darf nicht größer als 60 sein");
            }
            if (minute < 0)
            {
                throw new ArgumentOutOfRangeException("minute", "darf nicht kleiner als 0 sein");
            }
            if (minute < 60)
            {
                this.Hour = hour;
                this.Minute = minute;
            }
            else
            {
                this.Hour = hour + 1;
                this.Minute = 0;
            }
        }

        public int Hour { get; private set; }
        public int Minute { get; private set; }

        public static TimeItem Now
        {
            get
            {
                var now = DateTime.Now;
                int minutes = now.Minute - (now.Minute % 15);
                return new TimeItem(now.Hour, minutes);
            }
        }

        #region IComparable<TimeItem> Members

        public int CompareTo(TimeItem other)
        {
            if (other == null)
            {
                return 1;
            }
            int hourCompare = this.Hour.CompareTo(other.Hour);
            if (hourCompare == 0)
            {
                return this.Minute.CompareTo(other.Minute);
            }
            return hourCompare;
        }

        #endregion

        public bool IsBetween(TimeItem from, TimeItem to)
        {
            return this.CompareTo(from) > 0 && this.CompareTo(to) < 0;
        }

        public static bool TryParse(string s, out TimeItem ti)
        {
            bool success = false;
            ti = null;
            if (!string.IsNullOrEmpty(s))
            {
                var parts = s.Split(':').Select(p => p.Trim()).ToList();
                if (parts.Any())
                {
                    int hour;
                    if (int.TryParse(parts.ElementAt(0), out hour))
                    {
                        if (parts.Count() > 1)
                        {
                            int min;
                            if (int.TryParse(parts.ElementAt(1), out min))
                            {
                                ti = new TimeItem(hour, min);
                                success = true;
                            }
                        }
                        else
                        {
                            if (hour > 100 && hour <= 2400)
                            {
                                success = TryParse(hour.ToString("00:00"), out ti);
                            }
                            else
                            {
                                ti = new TimeItem(hour, 0);
                                success = true;
                            }
                        }
                    }
                }
            }
            return success;
        }

        public static TimeItem operator +(TimeItem ti, double hours)
        {
            int partBeforeKomma = (int)Math.Truncate(hours);
            double partAfterKomma = hours - partBeforeKomma;
            int minutes = (int)(Math.Round(partAfterKomma * 60) + ti.Minute);
            if (minutes >= 60)
            {
                partBeforeKomma++;
                minutes -= 60;
            }
            return new TimeItem(ti.Hour + partBeforeKomma, minutes);
        }

        public static TimeItem operator -(TimeItem ti, double hours)
        {
            int partBeforeKomma = (int)Math.Truncate(hours);
            double partAfterKomma = hours - partBeforeKomma;
            int minutes = (int)(ti.Minute - Math.Round(partAfterKomma * 60));
            if (minutes < 0)
            {
                partBeforeKomma++;
                minutes += 60;
            }
            return new TimeItem(ti.Hour - partBeforeKomma, minutes);
        }

        public static double operator -(TimeItem a, TimeItem b)
        {
            if (a != null && b != null)
            {
                if (a.CompareTo(b) == 0)
                {
                    return 0;
                }
                if (a.CompareTo(b) > 0)
                {
                    return b - a;
                }
                // do the real math, we know a is smaller than b here
                if (a.Hour == b.Hour)
                {
                    return (b.Minute - a.Minute) / 60d;
                }
                var minutes = 60 - a.Minute + b.Minute;
                var hours = b.Hour - (a.Hour + 1); // +1 because we took the minutes from the started hour into minutes
                return (hours * 60 + minutes) / 60d;
            }
            return 0;
        }


        public override bool Equals(object obj)
        {
            TimeItem ti = obj as TimeItem;
            if (ti != null)
            {
                return this.Hour == ti.Hour && this.Minute == ti.Minute;
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1:00}", this.Hour, this.Minute);
        }

        public string ToMonlistString()
        {
            return string.Format("{0:00}{1:00}", this.Hour, this.Minute);
        }

        public string ToShortString()
        {
            if (Minute == 0)
            {
                return string.Format("{0}", this.Hour);
            }
            return this.ToString();
        }
    }
}