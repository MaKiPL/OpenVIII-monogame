using System;

namespace OpenVIII.Fields
{
    public interface IFieldService
    {
        #region Properties

        EventEngine Engine { get; }
        bool IsSupported { get; }

        #endregion Properties

        #region Methods

        void BindArea(int areaId);

        void FadeIn();

        void FadeOff();

        void FadeOn();

        void FadeOut();

        void GoTo(int fieldId, int walkmeshId);

        void PrepareGoTo(int fieldId);

        #endregion Methods
    }
}