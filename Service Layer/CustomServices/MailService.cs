using DomainLayer.Common;
using DomainLayer.Data;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Service_Layer.ICustomServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
//using ConfigurationManager = WebAPI.ConfigurationManager;
namespace Service_Layer.CustomServices
{
    public class MailService : IMailService
    {
        private readonly MailSettings _settings;
        private readonly RxSplitterContext _context;

        public MailService(IOptions<MailSettings> settings, RxSplitterContext context)
        {
            _settings = settings.Value;
            _context = context;
        }

        public async Task<bool> SendAsync(MailData mailData, CancellationToken ct = default)
        {
            try
            {
                var mail = new MimeMessage();
                //email.Sender = MailboxAddress.Parse(_settings.From);
                mail.Sender = MailboxAddress.Parse("2017pcecsprabal115@poornima.org");
                //string toMail = "prabalsurana00@gmail.com";
                foreach (string mailAddress in mailData.To)
                    mail.To.Add(MailboxAddress.Parse(mailAddress));
                mail.Subject = mailData.Subject;
                //email.Subject = mailData.Subject;
                var body = new BodyBuilder();
                body.HtmlBody = mailData.Body;
                mail.Body = body.ToMessageBody();
                //builder.HtmlBody = mailData.Body;
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                smtp.Connect("smtp.gmail.com", _settings.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate("2017pcecsprabal115@poornima.org", "Prabalsuran");
                await smtp.SendAsync(mail);
                smtp.Disconnect(true);

                //// Initialize a new instance of the MimeKit.MimeMessage class
                //var mail = new MimeMessage();

                //#region Sender / Receiver
                //// Sender
                //mail.From.Add(new MailboxAddress(_settings.DisplayName, mailData.From ?? _settings.From));
                //mail.Sender = new MailboxAddress(mailData.DisplayName ?? _settings.DisplayName, mailData.From ?? _settings.From);

                //// Receiver
                //foreach (string mailAddress in mailData.To)
                //    mail.To.Add(MailboxAddress.Parse(mailAddress));

                //// Set Reply to if specified in mail data
                //if (!string.IsNullOrEmpty(mailData.ReplyTo))
                //    mail.ReplyTo.Add(new MailboxAddress(mailData.ReplyToName, mailData.ReplyTo));

                //// BCC
                //// Check if a BCC was supplied in the request
                //if (mailData.Bcc != null)
                //{
                //    // Get only addresses where value is not null or with whitespace. x = value of address
                //    foreach (string mailAddress in mailData.Bcc.Where(x => !string.IsNullOrWhiteSpace(x)))
                //        mail.Bcc.Add(MailboxAddress.Parse(mailAddress.Trim()));
                //}

                //// CC
                //// Check if a CC address was supplied in the request
                //if (mailData.Cc != null)
                //{
                //    foreach (string mailAddress in mailData.Cc.Where(x => !string.IsNullOrWhiteSpace(x)))
                //        mail.Cc.Add(MailboxAddress.Parse(mailAddress.Trim()));
                //}
                //#endregion

                //#region Content

                //// Add Content to Mime Message
                //var body = new BodyBuilder();
                //mail.Subject = mailData.Subject;
                //body.HtmlBody = mailData.Body;
                //mail.Body = body.ToMessageBody();

                //#endregion
                
                //        #region Send Mail

                //        using var smtp = new SmtpClient();

                //if (_settings.UseSSL)
                //{
                //    await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.SslOnConnect, ct);
                //}
                //else if (_settings.UseStartTls)
                //{
                //    await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, ct);
                //}
                //await smtp.AuthenticateAsync(_settings.UserName, _settings.Password, ct);
                //await smtp.SendAsync(mail, ct);
                //await smtp.DisconnectAsync(true, ct);

                //#endregion

                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
        //public async Task<bool> SendWithAttachmentsAsync(MailDataWithAttachments mailData, CancellationToken ct)
        //{
        //    try
        //    {
        //        // Initialize a new instance of the MimeKit.MimeMessage class
        //        var mail = new MimeMessage();

        //        #region Sender / Receiver
        //        // Sender
        //        mail.From.Add(new MailboxAddress(_settings.DisplayName, mailData.From ?? _settings.From));
        //        mail.Sender = new MailboxAddress(mailData.DisplayName ?? _settings.DisplayName, mailData.From ?? _settings.From);

        //        // Receiver
        //        foreach (string mailAddress in mailData.To)
        //            mail.To.Add(MailboxAddress.Parse(mailAddress));

        //        // Set Reply to if specified in mail data
        //        if (!string.IsNullOrEmpty(mailData.ReplyTo))
        //            mail.ReplyTo.Add(new MailboxAddress(mailData.ReplyToName, mailData.ReplyTo));

        //        // BCC
        //        // Check if a BCC was supplied in the request
        //        if (mailData.Bcc != null)
        //        {
        //            // Get only addresses where value is not null or with whitespace. x = value of address
        //            foreach (string mailAddress in mailData.Bcc.Where(x => !string.IsNullOrWhiteSpace(x)))
        //                mail.Bcc.Add(MailboxAddress.Parse(mailAddress.Trim()));
        //        }

        //        // CC
        //        // Check if a CC address was supplied in the request
        //        if (mailData.Cc != null)
        //        {
        //            foreach (string mailAddress in mailData.Cc.Where(x => !string.IsNullOrWhiteSpace(x)))
        //                mail.Cc.Add(MailboxAddress.Parse(mailAddress.Trim()));
        //        }
        //        #endregion

        //        #region Content

        //        // Add Content to Mime Message
        //        var body = new BodyBuilder();
        //        mail.Subject = mailData.Subject;
        //        body.HtmlBody = mailData.Body;
        //        mail.Body = body.ToMessageBody();

        //        // Check if we got any attachments and add the to the builder for our message
        //        if (mailData.Attachments != null)
        //        {
        //            byte[] attachmentFileByteArray;

        //            foreach (IFormFile attachment in mailData.Attachments)
        //            {
        //                // Check if length of the file in bytes is larger than 0
        //                if (attachment.Length > 0)
        //                {
        //                    // Create a new memory stream and attach attachment to mail body
        //                    using (MemoryStream memoryStream = new MemoryStream())
        //                    {
        //                        // Copy the attachment to the stream
        //                        attachment.CopyTo(memoryStream);
        //                        attachmentFileByteArray = memoryStream.ToArray();
        //                    }
        //                    // Add the attachment from the byte array
        //                    body.Attachments.Add(attachment.FileName, attachmentFileByteArray, ContentType.Parse(attachment.ContentType));
        //                }
        //            }
        //        }

        //        #endregion

        //        #region Send Mail

        //        using var smtp = new SmtpClient();

        //        if (_settings.UseSSL)
        //        {
        //            await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.SslOnConnect, ct);
        //        }
        //        else if (_settings.UseStartTls)
        //        {
        //            await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, ct);
        //        }

        //        await smtp.AuthenticateAsync(_settings.UserName, _settings.Password, ct);
        //        await smtp.SendAsync(mail, ct);
        //        await smtp.DisconnectAsync(true, ct);

        //        return true;
        //        #endregion

        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}
      
    }
}
