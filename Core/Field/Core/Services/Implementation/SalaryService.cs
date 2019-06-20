using System;

namespace OpenVIII
{
    public sealed class SalaryService : ISalaryService
    {
        public Boolean IsSupported => true;
        public Boolean IsSalaryEnabled { get; set; }
        public Boolean IsSalaryAlertEnabled { get; set; }
    }
}