using System;

namespace OpenVIII.Fields
{
    public interface IFieldService
    {
        Boolean IsSupported { get; }

        EventEngine Engine { get; }

        void FadeOn();
        void FadeOff();
        void FadeIn();
        void FadeOut();

        void PrepareGoTo(int fieldId);
        void GoTo(int fieldId, Int32 walkmeshId);
        void BindArea(Int32 areaId);
    }
}