namespace Accounting.Utilities.Internal.Interfaces
{
    public interface IUserContextRepository
    {
        string GetPredefinedParameterValue(byte PredefinedParameter_Id);
    }
}
