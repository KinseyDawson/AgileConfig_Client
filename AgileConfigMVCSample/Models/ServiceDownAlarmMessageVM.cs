﻿using System;

namespace AgileConfigMVCSample.Models
{
    public class ServiceDownAlarmMessageVM
    {
        public string UniqueId { get; set; }

        public string ServiceId { get; set; }

        public string ServiceName { get; set; }

        public DateTime Time { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }
    }
}
