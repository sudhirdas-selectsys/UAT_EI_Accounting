namespace Accounting.Entities.Internal.Enums
{
    public class Enum_PaymentStatus
    {
        public byte Completed { get; set; }
        public byte PendingSettlement { get; set; }
        public byte Voided { get; set; }
        public byte BouncedReversed { get; set; }
        public byte Failed { get; set; }
        public byte Cancelled { get; set; }
        public byte Pending { get; set; }
    }
}
