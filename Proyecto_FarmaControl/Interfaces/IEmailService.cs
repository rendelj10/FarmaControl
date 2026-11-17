
namespace Proyecto_FarmaControl.Interfaces
{
    public interface IEmailService
    {
        void SendPasswordResetEmail(
            string toEmail,
            string body);
    }
}
