using System;

namespace FF8.Core
{
    public sealed class SalaryService : ISalaryService
    {
        public Boolean IsSupported => true;
        public Boolean IsSalaryEnabled { get; set; }
        public Boolean IsSalaryAlertEnabled { get; set; }
    }
}