using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenVIII
{
    public class Input2
    {
        #region Fields

        private static readonly TimeSpan msDelayLimit = TimeSpan.FromMilliseconds(100);
        private static Input2 main;

        #endregion Fields

        #region Properties

        private static Dictionary<Button_Flags, FF8TextTagKey> Convert_Button { get; set; }

        #endregion Properties

        #region Methods

        private static void CheckInputLimit()
        {
            //issue here if CheckInputLimit is checked more than once per update cycle this will be wrong.
            if (Memory.gameTime != null)
                bLimitInput = (msDelay += Memory.gameTime.ElapsedGameTime) < msDelayLimit;
        }

        #endregion Methods

        protected static bool bLimitInput;
        protected static TimeSpan msDelay;

        protected bool ButtonTriggered(FF8TextTagKey key, ButtonTrigger trigger = ButtonTrigger.None)
        {
            if (Memory.IsActive)
                foreach (Inputs list in InputList)
                    foreach (KeyValuePair<List<FF8TextTagKey>, List<InputButton>> kvp in list.Data.Where(y => y.Key.Contains(key)))
                        foreach (InputButton test in kvp.Value)
                            if (main.ButtonTriggered(test, trigger))
                                return true;
            return false;
        }

        protected virtual bool UpdateOnce() => false;

        public Input2(bool skip = false)
        {
            Memory.Log.WriteLine($"{nameof(Input2)} :: {this}");
            if (!skip)
            {
                if (Keyboard == null)
                    Keyboard = new InputKeyboard();
                if (Mouse == null)
                    Mouse = new InputMouse();
                if (GamePad == null)
                    GamePad = new InputGamePad();
                if (InputList == null)
                {
                    InputList = new List<Inputs>
                    {
                        new Inputs_FF8PSX(),
                        new Inputs_OpenVIII(),
                        new Inputs_FF8Steam(),
                        new Inputs_FF82000()
                    };

                    //remove duplicate inputs.
                    int j = 1;
                    foreach (Inputs list in InputList)
                    {
                        for (int i = j; i < InputList.Count; i++)
                        {
                            foreach (KeyValuePair<List<FF8TextTagKey>, List<InputButton>> kvp in InputList[i].Data)
                            {
                                foreach (InputButton inputs in kvp.Value.ToArray())
                                    if (
                                        list.Data.Any(
                                            x => x.Value.Any(y => y.Equals(inputs)
                                            //x => x.Value.Any(y=>y.Key == inputs.Key &&
                                            //y.MouseButton == inputs.MouseButton &&
                                            //y.GamePadButton == inputs.GamePadButton &&
                                            ////y.Trigger == inputs.Trigger //&&
                                            //y.Combo == null//inputs.Combo
                                        )))
                                        kvp.Value.Remove(inputs);
                            }
                        }
                        j++;
                    }
                }

                if (main == null)
                    main = new Input2(true);
                if (Convert_Button == null)
                {
                    Convert_Button = new Dictionary<Button_Flags, FF8TextTagKey>()
                    {
                        //Buttons is
                        //finisher = 0x0001
                        //up = 0x0010
                        //-> = 0x0020
                        //do = 0x0040
                        //< - = 0x0080
                        //L2 = 0x0100
                        //R2 = 0x0200
                        //L1 = 0x0400
                        //R1 = 0x0800
                        // /\ = 0x1000
                        //O = 0x2000
                        //X = 0x4000
                        //| _ |= 0x8000
                        //None = 0xFFFF

                        {Button_Flags.Up, FF8TextTagKey.Up },
                        {Button_Flags.Right, FF8TextTagKey.Right },
                        {Button_Flags.Down, FF8TextTagKey.Down },
                        {Button_Flags.Left, FF8TextTagKey.Left },
                        {Button_Flags.L2, FF8TextTagKey.EscapeLeft },
                        {Button_Flags.R2, FF8TextTagKey.EscapeRight },
                        {Button_Flags.L1, FF8TextTagKey.RotateLeft },
                        {Button_Flags.R1, FF8TextTagKey.RotateRight },
                        {Button_Flags.Triangle, FF8TextTagKey.Cancel },
                        {Button_Flags.Circle, FF8TextTagKey.Menu },
                        {Button_Flags.Cross, FF8TextTagKey.Confirm },
                        {Button_Flags.Square, FF8TextTagKey.Cards }
                    };
                }
            }
        }

        public static InputGamePad GamePad { get; private set; }
        public static List<Inputs> InputList { get; private set; }
        public static InputKeyboard Keyboard { get; private set; }
        public static InputMouse Mouse { get; private set; }

        public static bool Button(FF8TextTagKey k, ButtonTrigger trigger = ButtonTrigger.None) => main?.ButtonTriggered(k, trigger) ?? false;

        public static bool Button(InputButton k, ButtonTrigger trigger = ButtonTrigger.None) => main?.ButtonTriggered(k, trigger) ?? false;

        public static bool Button(Keys k, ButtonTrigger trigger = ButtonTrigger.OnPress) => main?.ButtonTriggered(new InputButton() { Key = k, Trigger = trigger }) ?? false;

        public static bool Button(GamePadButtons k, ButtonTrigger trigger = ButtonTrigger.OnPress) => main?.ButtonTriggered(new InputButton() { GamePadButton = k, Trigger = trigger }) ?? false;

        public static bool Button(MouseButtons k, ButtonTrigger trigger = ButtonTrigger.OnPress) => main?.ButtonTriggered(new InputButton() { MouseButton = k, Trigger = trigger }) ?? false;

        public static bool Button(Button_Flags k, ButtonTrigger trigger = ButtonTrigger.OnPress) => Convert_Button.ContainsKey(k) && Button(Convert_Button[k], trigger);

        public static bool Button(List<FF8TextTagKey> k, ButtonTrigger trigger = ButtonTrigger.None) => k.Any(x => Button(x, trigger));

        public static FF8String ButtonString(FF8TextTagKey key, ButtonTrigger trigger = ButtonTrigger.None)
        {
            foreach (Inputs list in InputList)
                foreach (KeyValuePair<List<FF8TextTagKey>, List<InputButton>> kvp in list.Data.Where(y => y.Key.Contains(key)))
                    foreach (InputButton test in kvp.Value)
                        if (!list.DrawGamePadButtons)
                        {
                            return test.ToString();
                        }
                        else if (test.GamePadButton != GamePadButtons.None)
                        {
                            return GamePad.ButtonString(test.GamePadButton, key);
                        }
            return "";
        }

        public static IReadOnlyList<FF8TextTagKey> Convert_Flags(Button_Flags k)
        {
            List<FF8TextTagKey> ret = new List<FF8TextTagKey>(1);
            foreach (Button_Flags x in Enum.GetValues(typeof(Button_Flags)))
            {
                if (k.HasFlag(x) && (Convert_Button?.ContainsKey(k) ?? false))
                {
                    Debug.WriteLine("{0} set", x);
                    ret.Add(Convert_Button[k]);
                }
            }
            return ret;
        }

        public static bool DelayedButton(List<FF8TextTagKey> k, ButtonTrigger trigger = ButtonTrigger.OnPress) => k.Any(x => DelayedButton(x, trigger));

        public static bool DelayedButton(FF8TextTagKey k, ButtonTrigger trigger = ButtonTrigger.OnPress)
        {
            bool ret = Button(k, trigger);
            if (ret)
                ResetInputLimit();
            return ret;
        }

        public static bool DelayedButton(InputButton k, ButtonTrigger trigger = ButtonTrigger.OnPress)
        {
            bool ret = Button(k, trigger);
            if (ret)
                ResetInputLimit();
            return ret;
        }

        public static bool DelayedButton(Keys k, ButtonTrigger trigger = ButtonTrigger.OnPress)
        {
            bool ret = Button(k, trigger);
            if (ret)
                ResetInputLimit();
            return ret;
        }

        public static bool DelayedButton(MouseButtons k, ButtonTrigger trigger = ButtonTrigger.OnPress)
        {
            bool ret = Button(k, trigger);
            if (ret)
                ResetInputLimit();
            return ret;
        }

        public static bool DelayedButton(GamePadButtons k, ButtonTrigger trigger = ButtonTrigger.OnPress)
        {
            bool ret = Button(k, trigger);
            if (ret)
                ResetInputLimit();
            return ret;
        }

        public static bool DelayedButton(Button_Flags k, ButtonTrigger trigger = ButtonTrigger.OnPress) => Convert_Button.ContainsKey(k) && Button(Convert_Button[k], trigger);

        public static double Distance(float speed) =>
            // no input throttle but still take the max speed * time; for non analog controls
            speed * Memory.gameTime.ElapsedGameTime.TotalMilliseconds;

        public static void ResetInputLimit()
        {
            msDelay = TimeSpan.Zero;
            bLimitInput = false;
        }

        public static bool Update()
        {
            CheckInputLimit();
            Keyboard?.UpdateOnce();
            GamePad?.UpdateOnce();
            Mouse?.UpdateOnce();
            return false;
        }

        public virtual bool ButtonTriggered(InputButton test, ButtonTrigger trigger = ButtonTrigger.None)
        {
            if (Memory.IsActive)
                if (!bLimitInput || (trigger.HasFlag(ButtonTrigger.Force) ? trigger : (test.Trigger | trigger)).HasFlag(ButtonTrigger.IgnoreDelay))
                {
                    if (Keyboard.ButtonTriggered(test, trigger))
                        return true;
                    else if (Mouse.ButtonTriggered(test, trigger))
                        return true;
                    else if (GamePad.ButtonTriggered(test, trigger))
                        return true;
                }
            return false;
        }
    }
}