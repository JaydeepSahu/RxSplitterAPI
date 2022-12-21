using DomainLayer.Common;
using DomainLayer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Layer.ICustomServices
{
    public interface IMailService
    {
        Task<bool> SendAsync(MailData mailData, CancellationToken ct);
        

        //Task<bool> SendWithAttachmentsAsync(MailDataWithAttachments mailData, CancellationToken ct);
    }
}
