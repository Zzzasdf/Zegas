// using System;
//
// public class ClientDailyFirstRedPointConverter : BitmapTimingResetConverter
// {
//     protected override Func<long, bool> TimingResetFunc => OnTimingReset;
//     protected override Func<long> TimingAfterResetDataFunc => OnTimingAfterResetDataFunc;
//     protected override Action TimingAfterResetCallBack => OnTimingAfterResetCallBack;
//
//     private bool OnTimingReset(long lastData)
//     {
//         throw new NotImplementedException();
//         // return !SystemTime.Instance.IsToday(lastData);
//     }
//
//     private long OnTimingAfterResetDataFunc()
//     {
//         throw new NotImplementedException();
//         // return SystemTime.Instance.CurrentUnixTimeMilliseconds();
//     }
//     
//     private void OnTimingAfterResetCallBack()
//     {
//         GameEntry.ClientConverterCollectorSystem.Save((int)ClientConverterCollectorSystem.EDailyFirstRedDot.Unique_16);
//     }
//     
//     public bool this[EDailyFirstRedPoint flag]
//     {
//         get => HasFlag(flag);
//         set => SetFlag(flag, value);
//     }  
//     
//     public bool HasFlag(EDailyFirstRedPoint flag)
//     {
//         return HasFlag((uint)flag);
//     }
//         
//     public void SetFlag(EDailyFirstRedPoint flag, bool value)
//     {
//         SetFlag((uint)flag, value);
//     }
//
//     public void AddFlag(EDailyFirstRedPoint flag)
//     {
//         AddFlag((uint)flag);
//     }
//
//     public void RemoveFlag(EDailyFirstRedPoint flag)
//     {
//         RemoveFlag((uint)flag);
//     }
// }