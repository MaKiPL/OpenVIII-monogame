using System;

namespace OpenVIII
{
    public interface ISalaryService
    {
        Boolean IsSupported { get; }

        Boolean IsSalaryEnabled { get; set; }
        Boolean IsSalaryAlertEnabled { get; set; }
    }
}