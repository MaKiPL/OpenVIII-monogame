using System;

namespace OpenVIII.Fields
{
    public sealed class SalaryService : ISalaryService
    {
        #region Properties

        public bool IsSalaryAlertEnabled { get; set; }
        public bool IsSalaryEnabled { get; set; }
        public bool IsSupported => true;

        #endregion Properties
    }
}