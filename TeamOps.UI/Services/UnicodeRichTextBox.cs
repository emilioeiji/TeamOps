using System;
using System.Runtime.InteropServices;
namespace TeamOps.UI.Forms;

public class UnicodeRichTextBox : RichTextBox
{
    private const int EM_SETCHARFORMAT = 1092;
    private const int SCF_ALL = 0x0004;
    private const uint CFM_UNICODE = 0x00000008;

    [StructLayout(LayoutKind.Sequential)]
    private struct CHARFORMAT2
    {
        public int cbSize;
        public uint dwMask;
        public uint dwEffects;
        public int yHeight;
        public int yOffset;
        public int crTextColor;
        public byte bCharSet;
        public byte bPitchAndFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szFaceName;
        public ushort wWeight;
        public short sSpacing;
        public int crBackColor;
        public int lcid;
        public int dwReserved;
        public short sStyle;
        public short wKerning;
        public byte bUnderlineType;
        public byte bAnimation;
        public byte bRevAuthor;
        public byte bReserved1;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(
        IntPtr hWnd, int msg, int wParam, ref CHARFORMAT2 lParam);

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        var cf = new CHARFORMAT2();
        cf.cbSize = Marshal.SizeOf(cf);
        cf.dwMask = CFM_UNICODE;

        SendMessage(this.Handle, EM_SETCHARFORMAT, SCF_ALL, ref cf);
    }
}
