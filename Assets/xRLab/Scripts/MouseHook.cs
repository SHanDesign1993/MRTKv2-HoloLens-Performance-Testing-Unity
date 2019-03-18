using System;

namespace MMFrame.Windows.GlobalHook
{
    /// <summary>
    /// マウスのグローバルフックに関するクラス
    /// </summary>
    public static class MouseHook
    {
        /// <summary>
        /// P/Invoke
        /// </summary>
        private static class NativeMethods
        {
            /// <summary>
            /// フックプロシージャのデリゲート
            /// </summary>
            /// <param name="nCode">フックプロシージャに渡すフックコード</param>
            /// <param name="msg">フックプロシージャに渡す値</param>
            /// <param name="msllhookstruct">フックプロシージャに渡す値</param>
            /// <returns>フックチェーン内の次のフックプロシージャの戻り値</returns>
            public delegate System.IntPtr MouseHookCallback(int nCode, uint msg, ref MSLLHOOKSTRUCT msllhookstruct);

            /// <summary>
            /// アプリケーション定義のフックプロシージャをフックチェーン内にインストールします。
            /// フックプロシージャをインストールすると、特定のイベントタイプを監視できます。
            /// 監視の対象になるイベントは、特定のスレッド、または呼び出し側スレッドと同じデスクトップ内のすべてのスレッドに関連付けられているものです。
            /// </summary>
            /// <param name="idHook">フックタイプ</param>
            /// <param name="lpfn">フックプロシージャ</param>
            /// <param name="hMod">アプリケーションインスタンスのハンドル</param>
            /// <param name="dwThreadId">スレッドの識別子</param>
            /// <returns>関数が成功すると、フックプロシージャのハンドルが返ります。関数が失敗すると、NULL が返ります。</returns>
            [System.Runtime.InteropServices.DllImport("user32")]
            public static extern System.IntPtr SetWindowsHookEx(int idHook, MouseHookCallback lpfn, System.IntPtr hMod, uint dwThreadId);

            /// <summary>
            /// 現在のフックチェーン内の次のフックプロシージャに、フック情報を渡します。
            /// フックプロシージャは、フック情報を処理する前でも、フック情報を処理した後でも、この関数を呼び出せます。
            /// </summary>
            /// <param name="hhk">現在のフックのハンドル</param>
            /// <param name="nCode">フックプロシージャに渡すフックコード</param>
            /// <param name="msg">フックプロシージャに渡す値</param>
            /// <param name="msllhookstruct">フックプロシージャに渡す値</param>
            /// <returns>フックチェーン内の次のフックプロシージャの戻り値</returns>
            [System.Runtime.InteropServices.DllImport("user32")]
            public static extern System.IntPtr CallNextHookEx(System.IntPtr hhk, int nCode, uint msg, ref MSLLHOOKSTRUCT msllhookstruct);

            /// <summary>
            /// SetWindowsHookEx 関数を使ってフックチェーン内にインストールされたフックプロシージャを削除します。
            /// </summary>
            /// <param name="hhk">削除対象のフックプロシージャのハンドル</param>
            /// <returns>関数が成功すると、0 以外の値が返ります。関数が失敗すると、0 が返ります。</returns>
            [System.Runtime.InteropServices.DllImport("user32")]
            [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
            public static extern bool UnhookWindowsHookEx(System.IntPtr hhk);


            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool SetCursorPos(int x, int y);

        }


        /// <summary>
        /// マウスの状態の構造体
        /// </summary>
        public struct StateMouse
        {
            public Stroke Stroke;
            public int X;
            public int Y;
            public uint Data;
            public uint Flags;
            public uint Time;
            public System.IntPtr ExtraInfo;
        }

        /// <summary>
        /// 挙動の列挙型
        /// </summary>
        public enum Stroke
        {
            MOVE,
            LEFT_DOWN,
            LEFT_UP,
            RIGHT_DOWN,
            RIGHT_UP,
            MIDDLE_DOWN,
            MIDDLE_UP,
            WHEEL_DOWN,
            WHEEL_UP,
            X1_DOWN,
            X1_UP,
            X2_DOWN,
            X2_UP,
            UNKNOWN
        }

        /// <summary>
        /// マウスのグローバルフックを実行しているかどうかを取得、設定します。
        /// </summary>
        public static bool IsHooking
        {
            get;
            private set;
        }

        /// <summary>
        /// マウスのグローバルフックを中断しているかどうかを取得、設定します。
        /// </summary>
        public static bool IsPaused
        {
            get;
            private set;
        }

        /// <summary>
        /// マウスの状態を取得、設定します。
        /// </summary>
        public static StateMouse State;

        /// <summary>
        /// フックプロシージャ内でのイベント用のデリゲート
        /// </summary>
        /// <param name="msg">マウスに関するウィンドウメッセージ</param>
        /// <param name="msllhookstruct">低レベルのマウスの入力イベントの構造体</param>
        public delegate void HookHandler(ref StateMouse state);

        /// <summary>
        /// x 座標と y 軸座標の構造体
        /// </summary>
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        /// <summary>
        /// 低レベルのマウスの入力イベントの構造体
        /// </summary>
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public System.IntPtr dwExtraInfo;
        }

        /// <summary>
        /// フックプロシージャのハンドル
        /// </summary>
        private static System.IntPtr Handle;

        /// <summary>
        /// 入力をキャンセルするかどうかを取得、設定します。
        /// </summary>
        private static bool IsCancel;

        /// <summary>
        /// 登録イベントのリストを取得、設定します。
        /// </summary>
        private static System.Collections.Generic.List<HookHandler> Events;

        /// <summary>
        /// フックプロシージャ内でのイベント
        /// </summary>
        private static event HookHandler HookEvent;

        /// <summary>
        /// フックチェーンにインストールするフックプロシージャのイベント
        /// </summary>
        private static event NativeMethods.MouseHookCallback hookCallback;

        /// <summary>
        /// フックプロシージャをフックチェーン内にインストールし、マウスのグローバルフックを開始します。
        /// </summary>
        /// <exception cref="System.ComponentModel.Win32Exception"></exception>
        public static void Start()
        {
            if (IsHooking)
            {
                return;
            }

            IsHooking = true;
            IsPaused = false;

            hookCallback = HookProcedure;
            System.IntPtr h = IntPtr.Zero;
            Handle = NativeMethods.SetWindowsHookEx(7, hookCallback, h, (uint)AppDomain.GetCurrentThreadId());

            if (Handle == System.IntPtr.Zero)
            {
                IsHooking = false;
                IsPaused = true;

                throw new System.ComponentModel.Win32Exception();

            }
        }

        /// <summary>
        /// マウスのグローバルフックを停止し、フックプロシージャをフックチェーン内から削除します。
        /// </summary>
        public static void Stop()
        {
            if (!IsHooking)
            {
                return;
            }

            if (Handle != System.IntPtr.Zero)
            {
                IsHooking = false;
                IsPaused = true;

                ClearEvent();

                NativeMethods.UnhookWindowsHookEx(Handle);
                Handle = System.IntPtr.Zero;
                hookCallback -= HookProcedure;
            }
        }

        /// <summary>
        /// 次のフックプロシージャにフック情報を渡すようにします。
        /// </summary>
        public static void Enable()
        {
            IsCancel = false;
        }

        /// <summary>
        /// 次のフックプロシージャにフック情報を渡すのをキャンセルします。
        /// </summary>
        public static void Disable()
        {
            IsCancel = true;
        }

        /// <summary>
        /// マウスのグローバルフックを中断します。
        /// </summary>
        public static void Pause()
        {
            IsPaused = true;
        }

        /// <summary>
        /// マウス操作時のイベントを追加します。
        /// </summary>
        /// <param name="hookHandler"></param>
        public static void AddEvent(HookHandler hookHandler)
        {
            if (Events == null)
            {
                Events = new System.Collections.Generic.List<HookHandler>();
            }

            Events.Add(hookHandler);
            HookEvent += hookHandler;
        }

        /// <summary>
        /// マウス操作時のイベントを削除します。
        /// </summary>
        /// <param name="hookHandler"></param>
        public static void RemoveEvent(HookHandler hookHandler)
        {
            if (Events == null)
            {
                return;
            }

            HookEvent -= hookHandler;
            Events.Remove(hookHandler);
        }

        /// <summary>
        /// マウス操作時のイベントを全て削除します。
        /// </summary>
        public static void ClearEvent()
        {
            if (Events == null)
            {
                return;
            }

            foreach (HookHandler e in Events)
            {
                HookEvent -= e;
            }

            Events.Clear();
        }

        /// <summary>
        /// フックチェーンにインストールするフックプロシージャ
        /// </summary>
        /// <param name="nCode">フックプロシージャに渡すフックコード</param>
        /// <param name="msg">フックプロシージャに渡す値</param>
        /// <param name="msllhookstruct">フックプロシージャに渡す値</param>
        /// <returns>フックチェーン内の次のフックプロシージャの戻り値</returns>
        private static System.IntPtr HookProcedure(int nCode, uint msg, ref MSLLHOOKSTRUCT s)
        {
            if (nCode >= 0 && HookEvent != null && !IsPaused)
            {
                State.Stroke = GetStroke(msg, ref s);
                State.X = s.pt.x;
                State.Y = s.pt.y;
                State.Data = s.mouseData;
                State.Flags = s.flags;
                State.Time = s.time;
                State.ExtraInfo = s.dwExtraInfo;

                HookEvent(ref State);

                if (IsCancel)
                {
                    return (System.IntPtr)1;
                }
            }
            return NativeMethods.CallNextHookEx(Handle, nCode, msg, ref s);
        }

        /// <summary>
        /// マウスボタンの挙動を取得します。
        /// </summary>
        /// <param name="msg">マウスに関するウィンドウメッセージ</param>
        /// <param name="s">低レベルのマウスの入力イベントの構造体</param>
        /// <returns>マウスボタンの挙動</returns>
        private static Stroke GetStroke(uint msg, ref MSLLHOOKSTRUCT s)
        {
            switch (msg)
            {
                case 0x0200:
                    // WM_MOUSEMOVE
                    return Stroke.MOVE;
                case 0x0201:
                    // WM_LBUTTONDOWN
                    return Stroke.LEFT_DOWN;
                case 0x0202:
                    // WM_LBUTTONUP
                    return Stroke.LEFT_UP;
                case 0x0204:
                    // WM_RBUTTONDOWN
                    return Stroke.RIGHT_DOWN;
                case 0x0205:
                    // WM_RBUTTONUP
                    return Stroke.RIGHT_UP;
                case 0x0207:
                    // WM_MBUTTONDOWN
                    return Stroke.MIDDLE_DOWN;
                case 0x0208:
                    // WM_MBUTTONUP
                    return Stroke.MIDDLE_UP;
                case 0x020A:
                    // WM_MOUSEWHEE
                    return ((short)((s.mouseData >> 16) & 0xffff) > 0) ? Stroke.WHEEL_UP : Stroke.WHEEL_DOWN;
                case 0x20B:
                    // WM_XBUTTONDOWN
                    switch (s.mouseData >> 16)
                    {
                        case 1:
                            return Stroke.X1_DOWN;
                        case 2:
                            return Stroke.X2_DOWN;
                        default:
                            return Stroke.UNKNOWN;
                    }
                case 0x20C:
                    // WM_XBUTTONUP
                    switch (s.mouseData >> 16)
                    {
                        case 1:
                            return Stroke.X1_UP;
                        case 2:
                            return Stroke.X2_UP;
                        default:
                            return Stroke.UNKNOWN;
                    }
                default:
                    return Stroke.UNKNOWN;
            }
        }

        public static void SetCursorPos(int x, int y)
        {
            NativeMethods.SetCursorPos(x, y);
        }

    }
}