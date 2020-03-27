using System;

namespace OpenVIII.Fields
{
    public interface ISalaryService
    {
        #region Properties

        bool IsSalaryAlertEnabled { get; set; }
        bool IsSalaryEnabled { get; set; }
        bool IsSupported { get; }

        #endregion Properties
    }
}