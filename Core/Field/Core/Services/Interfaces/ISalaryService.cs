using System;

namespace FF8
{
    public interface ISalaryService
    {
        Boolean IsSupported { get; }

        Boolean IsSalaryEnabled { get; set; }
        Boolean IsSalaryAlertEnabled { get; set; }
    }
}