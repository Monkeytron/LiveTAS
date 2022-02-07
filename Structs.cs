using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace LiveTAS
{
    [StructLayout(LayoutKind.Sequential)] public struct KeyboardInput
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;

        public static KeyboardInput KeyPress(VirtualKey vkey, bool press)
        {
            KeyboardInput input = new KeyboardInput
            {
                wScan = (ushort)Methods.MapVirtualKeyA((uint)vkey, 0),
                wVk = (ushort)vkey,
                dwFlags = (ushort)((press ? KeyEventF.KeyDown : KeyEventF.KeyUp) | KeyEventF.Scancode),
                dwExtraInfo = Methods.GetMessageExtraInfo()
            };
            if (needsExtendedKey.Contains(vkey)) input.dwFlags |= (ushort)KeyEventF.ExtendedKey;
            return input;
        }
        

        public static List<VirtualKey> needsExtendedKey = new List<VirtualKey>()
        {
            VirtualKey.RCONTROL,
            VirtualKey.SUBTRACT,
            VirtualKey.RMENU,
            VirtualKey.CANCEL,
            VirtualKey.HOME,
            VirtualKey.UP,
            VirtualKey.PRIOR,
            VirtualKey.LEFT,
            VirtualKey.RIGHT,
            VirtualKey.END,
            VirtualKey.DOWN,
            VirtualKey.NEXT,
            VirtualKey.INSERT,
            VirtualKey.DELETE,
            VirtualKey.LWIN,
            VirtualKey.RWIN,
            VirtualKey.MENU
        }; // These keys share scancodes with other keys, so KeyEventF.ExtendedKey must be set for these to specify which one you want.
    } // Struct to contain keyboard input data
    [StructLayout(LayoutKind.Sequential)]public struct MouseInput
    {
        public int dx;
        public int dy;
        public int mouseData;
        public int dwFlags;
        public int time;
        public IntPtr dwExtraInfo;

        public static MouseInput MoveTo(POINT position)
        {
            MouseInput input = new MouseInput
            {
                dx = position.x,
                dy = position.y,
                dwExtraInfo = Methods.GetMessageExtraInfo(),
                dwFlags = (int)(MouseEventF.Absolute | MouseEventF.Move)
            };
            return input;
        }
        public static MouseInput Scroll(int amount)
        {
            MouseInput input = new MouseInput
            {
                mouseData = amount,
                dwExtraInfo = Methods.GetMessageExtraInfo(),
                dwFlags = (int)MouseEventF.Wheel
            };
            return input;
        }
    } // Struct to contain mouse input data
    [StructLayout(LayoutKind.Sequential)]public struct HardwareInput
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    } // Struct to contain hardware input data 
    [StructLayout(LayoutKind.Explicit)]public struct InputUnion
    {
        [FieldOffset(0)] public MouseInput mi;
        [FieldOffset(0)] public KeyboardInput ki;
        [FieldOffset(0)] public HardwareInput hi;
    } // Struct to contain one of the above input types.
    public struct Input
    {
        public int type;
        public InputUnion u;
    } // Struct to contain an InputUnion and which input type it refers to.

    public struct InputTime
    {
        public Input input;
        public TimeSpan Timestamp;
    }

    [Flags]public enum InputType
    {
        Mouse = 0,
        Keyboard = 1,
        Hardware = 2
    }
    [Flags]public enum KeyEventF
    {
        KeyDown = 0x0000,
        ExtendedKey = 0x0001,
        KeyUp = 0x0002,
        Unicode = 0x0004,
        Scancode = 0x0008
    }
    [Flags]public enum MouseEventF
    {
        Move = 0x0001,
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        RightDown = 0x0008,
        RightUp = 0x0010,
        MiddleDown = 0x0020,
        MiddleUp = 0x0040,
        XDown = 0x0080,
        XUp = 0x0100,
        Wheel = 0x0800,
        HWheel = 0x1000,
        MoveNoCoalesce = 0x2000,
        VirtualDesk = 0x4000,
        Absolute = 0x8000
    }

    public enum VirtualKey
    {
        /// <summary>
        /// Left mouse button
        /// </summary>
        LBUTTON = 0x01,

        /// <summary>
        /// Right mouse button
        /// </summary>
        RBUTTON = 0x02,

        /// <summary>
        /// Control - break processing
        /// </summary>
        CANCEL = 0x03,

        /// <summary>
        /// Middle mouse button
        /// </summary>
        MBUTTON = 0x04,

        /// <summary>
        /// X1 mouse button
        /// </summary>
        XBUTTON1 = 0x05,

        /// <summary>
        /// X2 mouse button
        /// </summary>
        XBUTTON2 = 0x06,

        /// <summary>
        /// BACKSPACE key
        /// </summary>
        BACK = 0x08,

        TAB = 0x09,

        CLEAR = 0x0C,

        /// <summary>
        /// ENTER key
        /// </summary>
        RETURN = 0x0D, 

        SHIFT = 0x10,

        CONTROL = 0x11,

        /// <summary>
        /// ALT key
        /// </summary>
        MENU = 0x12,

        PAUSE = 0x13,

        /// <summary>
        /// CAPS LOCK key
        /// </summary>
        CAPITAL = 0x14,

        KANA = 0x15,
        HANGUEL = 0x15,
        HANGUL = 0x15,
        IME_ON = 0x16,
        JUNJA = 0x17,
        FINAL = 0x18,
        HANJA = 0x19,
        KANJI = 0x19,
        IME_OFF = 0x1A,

        /// <summary>
        /// ESC key
        /// </summary>
        ESCAPE = 0x1B,

        CONVERT = 0x1C,
        NONCONVERT = 0x1D,
        ACCEPT = 0x1E,
        MODECHANGE = 0x1F,

        /// <summary>
        /// SPACEBAR
        /// </summary>
        SPACE = 0x20,

        /// <summary>
        /// PAGE UP
        /// </summary>
        PRIOR = 0x21,

        /// <summary>
        /// PAGE DOWN
        /// </summary>
        NEXT = 0x22,

        END = 0x23,

        HOME = 0x24,

        LEFT = 0x25,

        UP = 0x26,

        RIGHT = 0x27,

        DOWN = 0x28,

        SELECT = 0x29,

        PRINT = 0x2A,

        EXECUTE = 0x2B,

        /// <summary>
        /// PRINT SCREEN key
        /// </summary>
        SNAPSHOT = 0x2C,

        INSERT = 0x2D,

        DELETE = 0x2E,

        HELP = 0x2F,

        NUMBER0 = 0x30,
        NUMBER1 = 0x31,
        NUMBER2 = 0x32,
        NUMBER3 = 0x33,
        NUMBER4 = 0x34,
        NUMBER5 = 0x35,
        NUMBER6 = 0x36,
        NUMBER7 = 0x37,
        NUMBER8 = 0x38,
        NUMBER9 = 0x39,

        A = 0x41,
        B = 0x42,
        C = 0x43,
        D = 0x44,
        E = 0x45,
        F = 0x46,
        G = 0x47,
        H = 0x48,
        I = 0x49,
        J = 0x4A,
        K = 0x4B,
        L = 0x4C,
        M = 0x4D,
        N = 0x4E,
        O = 0x4F,
        P = 0x50,
        Q = 0x51,
        R = 0x52,
        S = 0x53,
        T = 0x54,
        U = 0x55,
        V = 0x56,
        W = 0x57,
        X = 0x58,
        Y = 0x59,
        Z = 0x5A,

        LWIN = 0x5B,

        RWIN = 0x5C,

        APPS = 0x5D,

        SLEEP = 0x5F,

        NUMPAD0 = 0x60,
        NUMPAD1 = 0x61,
        NUMPAD2 = 0x62,
        NUMPAD3 = 0x63,
        NUMPAD4 = 0x64,
        NUMPAD5 = 0x65,
        NUMPAD6 = 0x66,
        NUMPAD7 = 0x67,
        NUMPAD8 = 0x68,
        NUMPAD9 = 0x69,

        /// <summary>
        /// Keypad *
        /// </summary>
        MULTIPLY = 0x6A,

        /// <summary>
        /// Keypad +
        /// </summary>
        ADD = 0x6B,
        
        /// <summary>
        /// Not sure what key this is
        /// </summary>
        SEPARATOR = 0x6C,

        /// <summary>
        /// Keypad -
        /// </summary>
        SUBTRACT = 0x6D,

        /// <summary>
        /// Keypad .
        /// </summary>
        DECIMAL = 0x6E,

        /// <summary>
        /// Keypad /
        /// </summary>
        DIVIDE = 0x6F,

        F1 = 0x70,
        F2 = 0x71,
        F3 = 0x72,
        F4 = 0x73,
        F5 = 0x74,
        F6 = 0x75,
        F7 = 0x76,
        F8 = 0x77,
        F9 = 0x78,
        F10 = 0x79,
        F11 = 0x7A,
        F12 = 0x7B,
        F13 = 0x7C,
        F14 = 0x7D,
        F15 = 0x7E,
        F16 = 0x7F,
        F17 = 0x80,
        F18 = 0x81,
        F19 = 0x82,
        F20 = 0x83,
        F21 = 0x84,
        F22 = 0x85,
        F23 = 0x86,
        F24 = 0x87,

        NUMLOCK = 0x90,

        /// <summary>
        /// Scroll lock
        /// </summary>
        SCROLL = 0x91,

        LSHIFT = 0xA0,
        RSHIFT = 0xA1,
        LCONTROL = 0xA2,
        RCONTROL = 0xA3,
        LMENU = 0xA4,
        RMENU = 0xA5,

        BROWSER_BACK = 0xA6,
        BROWSER_FORWARD = 0xA7,
        BROWSER_REFRESH = 0xA8,
        BROWSER_STOP = 0xA9,
        BROWSER_SEARCH = 0xAA,
        BROWSER_FAVOURITES = 0xAB,
        BROWSER_HOME = 0xAC,

        VOLUME_MUTE = 0xAD,
        VOLUME_DOWN = 0xAE,
        VOLUME_UP = 0xAF,
        MEDIA_NEXT_TRACK = 0xB0,
        MEDIA_PREV_TRACK = 0xB1,
        MEDIA_STOP = 0xB2,
        MEDIA_PLAY_PAUSE = 0xB3,

        LAUNCH_MAIL = 0xB4,
        LAUNCH_MEDIA_SELECT = 0xB5,
        LAUNCH_APP1 = 0xB6,
        LAUNCH_APP2 = 0xB7,

        /// <summary>
        /// OEM keys do different things on different computers.
        /// </summary>
        OEM_1 = 0xBA,
        /// <summary>
        /// OEM keys do different things on different computers.
        /// </summary>
        OEM_PLUS = 0xBB,
        /// <summary>
        /// OEM keys do different things on different computers.
        /// </summary>
        OEM_COMMA = 0xBC,
        /// <summary>
        /// OEM keys do different things on different computers.
        /// </summary>
        OEM_MINUS = 0xBD,
        /// <summary>
        /// OEM keys do different things on different computers.
        /// </summary>
        OEM_PERIOD = 0xBE,
        /// <summary>
        /// OEM keys do different things on different computers.
        /// </summary>
        OEM_2 = 0xBF,
        /// <summary>
        /// OEM keys do different things on different computers.
        /// </summary>
        OEM_3 = 0xC0,
        /// <summary>
        /// OEM keys do different things on different computers.
        /// </summary>
        OEM_4 = 0xDB,
        /// <summary>
        /// OEM keys do different things on different computers.
        /// </summary>
        OEM_5 = 0xDC,
        /// <summary>
        /// OEM keys do different things on different computers.
        /// </summary>
        OEM_6 = 0xDD,
        /// <summary>
        /// OEM keys do different things on different computers.
        /// </summary>
        OEM_7 = 0xDE,
        /// <summary>
        /// OEM keys do different things on different computers.
        /// </summary>
        OEM_8 = 0xDF,
        /// <summary>
        /// OEM keys do different things on different computers.
        /// </summary>
        OEM_102 = 0xE2,

        PROCESSKEY = 0xE5,
        ATTN = 0xF6,
        CRSEL = 0xF7,
        EXSEL = 0xF8,
        EREOF = 0xF9,
        PLAY = 0xFA,
        ZOOM = 0xFB,
        PA1 = 0xFD,
        OEM_CLEAR = 0xFE
    } // All comments are the key registered on my keyboard.

    public struct POINT
    {
        public int x;
        public int y;

        public override string ToString()
        {
            return $"({x},{y})";
        }

        public static bool operator ==(POINT point, POINT otherPoint)
        {
            return point.x == otherPoint.x && point.y == otherPoint.y;
        }
        public static bool operator !=(POINT point, POINT otherPoint)
        {
            return !(point == otherPoint);
        }
    }

    public struct RecordFrame
    {
        public TimeSpan timeStamp;
        public List<VirtualKey> heldKeys;
        public POINT scaledMousePos;

        public RecordFrame(TimeSpan timestamp, POINT scaledmousepos)
        {
            timeStamp = timestamp;
            heldKeys = new List<VirtualKey>();
            scaledMousePos = scaledmousepos;
        }
    }
}
