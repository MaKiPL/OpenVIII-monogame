using System;

namespace FF8.Core
{
    public interface IFieldService
    {
        Boolean IsSupported { get; }

        EventEngine Engine { get; }

        void FadeOn();
        void FadeOff();
        void FadeIn();
        void FadeOut();

        void PrepareGoTo(FieldId fieldId);
        void GoTo(FieldId fieldId, Int32 walkmeshId);
        void BindArea(Int32 areaId);
    }
}