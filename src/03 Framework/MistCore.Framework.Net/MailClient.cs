using Microsoft.Extensions.Configuration;
using MistCore.Core.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MistCore.Framework.Net
{
    public class MailClient
    {
        public string DisplayName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public bool IsEnableSSL { get; set; }
        public string FromMail { get; set; }


        public Action<MailResultMessaage> SendMailComplate;

        public MailClient(MailServer mailServer)
        {
            BindMailServer(mailServer.Host, mailServer.Port, mailServer.IsEnableSSL, mailServer.Address, mailServer.Password, mailServer.DisplayName, mailServer.FromMail);
        }

        public MailClient(string mailServerName)
        {
            if (string.IsNullOrEmpty(mailServerName))
            {
                mailServerName = "MailServer";
            }

            var mailServer = new MailServer();
            GlobalConfiguration.Configuration.GetSection(mailServerName).Bind(mailServer);

            BindMailServer(mailServer.Host, mailServer.Port, mailServer.IsEnableSSL, mailServer.Address, mailServer.Password, mailServer.DisplayName, mailServer.FromMail);
        }

        public MailClient(string host, int port, bool isEnableSSL, string address, string password, string displayName, string fromMail = null)
        {
            BindMailServer(host, port, isEnableSSL, address, password, displayName, fromMail);
        }

        private void BindMailServer(string host, int port, bool isEnableSSL, string address, string password, string displayName, string fromMail = null)
        {
            this.DisplayName = displayName;
            this.Host = host;
            this.Port = port;
            this.Address = address;
            this.Password = password;
            this.IsEnableSSL = IsEnableSSL;
            this.FromMail = fromMail;

            this.FromMail = string.IsNullOrWhiteSpace(this.FromMail) ? this.Address : this.FromMail;
        }

        /// <summary>
        /// 发送电子邮件
        /// </summary>
        /// <param name="fromEmail">发件人</param>
        /// <param name="toEmail">收件人</param>
        /// <param name="subj">主题</param>
        /// <param name="bodys">内容</param>
        /// <param name="atts">stream, name, mediaType</param>
        public void SendMail(string toMail, string subj, string bodys, List<MailAttachment> atts = null, string fromMail = null)
        {
            Stopwatch watch = null;
            if (SendMailComplate != null)
            {
                watch = new Stopwatch();
                watch.Start();
            }

            var mr = new MailResultMessaage
            {
                ToMail = toMail,
                Subject = subj,
                Bodys = bodys,
                FromMail = fromMail ?? FromMail,
                MailServer = Address,
                Attachments = atts,
                SendTime = DateTime.Now,
            };

            try
            {
                SendMail(Host, Port, IsEnableSSL, Address, Password, DisplayName, fromMail ?? FromMail, toMail, subj, bodys, atts);
                mr.Status = 1;
            }
            catch (Exception ex)
            {
                mr.Status = 0;
                mr.ExceptionInfo = ex.Message;
            }
            finally
            {
                if (SendMailComplate != null)
                {
                    mr.ElapsedTime = watch.ElapsedMilliseconds;
                    SendMailComplate(mr);
                }
            }
        }

        /// <summary>
        /// 发送电子邮件
        /// </summary>
        /// <param name="fromEmail">发件人</param>
        /// <param name="toEmail">收件人</param>
        /// <param name="subj">主题</param>
        /// <param name="bodys">内容</param>
        public async Task SendMailAsync(string toMail, string subj, string bodys, string fromMail = null)
        {
            Stopwatch watch = null;
            if (SendMailComplate != null)
            {
                watch = new Stopwatch();
                watch.Start();
            }

            var mr = new MailResultMessaage
            {
                ToMail = toMail,
                Subject = subj,
                Bodys = bodys,
                FromMail = fromMail ?? FromMail,
                MailServer = Address,
                SendTime = DateTime.Now,
            };

            try
            {
                await SendMailAsync(Host, Port, IsEnableSSL, Address, Password, DisplayName, fromMail ?? FromMail, toMail, subj, bodys);
                mr.Status = 1;
            }
            catch (Exception ex)
            {
                mr.Status = 0;
                mr.ExceptionInfo = ex.Message;
            }
            finally
            {
                if (SendMailComplate != null)
                {
                    mr.ElapsedTime = watch.ElapsedMilliseconds;
                    SendMailComplate(mr);
                }
            }
        }

        /// <summary>
        /// 发送电子邮件
        /// </summary>
        /// <param name="smtpserver">SMTP服务器</param>
        /// <param name="enableSsl">是否启用SSL加密</param>
        /// <param name="userName">登录帐号</param>
        /// <param name="pwd">登录密码</param>
        /// <param name="nickName">发件人昵称</param>
        /// <param name="fromEmail">发件人</param>
        /// <param name="toEmail">收件人</param>
        /// <param name="subj">主题</param>
        /// <param name="bodys">内容</param>
        /// <param name="attachments">stream, name, mediaType</param>
        private static void SendMail(string smtpserver, int? prot, bool enableSsl, string userName, string pwd, string nickName, string fromMail, string toMail, string subj, string bodys,
            List<MailAttachment> attachments = null)
        {
            var smtpClient = new SmtpClient();
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式
            smtpClient.Host = smtpserver;//指定SMTP服务器
            if (prot != null)
            {
                smtpClient.Port = prot.Value;
            }

            smtpClient.Credentials = new NetworkCredential(userName, pwd);//用户名和密码
            smtpClient.EnableSsl = enableSsl;

            MailMessage mailMessage = new MailMessage();

            MailAddress fromAddress = new MailAddress(fromMail, nickName);
            mailMessage.From = fromAddress;

            var toAddresss = toMail.Split(new[] { ',', ';' }).Where(c => !string.IsNullOrEmpty(c)).Select(c => new MailAddress(c)).ToList();
            foreach (var toAddress in toAddresss)
            {
                mailMessage.To.Add(toAddress);
            }

            var atts = attachments?.Select(c => new Attachment(c.Stream, c.Name, c.MediaType)).ToList() ?? new List<Attachment>();
            foreach (var att in atts)
            {
                mailMessage.Attachments.Add(att);
            }
            mailMessage.Subject = subj;//主题
            mailMessage.Body = bodys;//内容
            mailMessage.BodyEncoding = Encoding.Default;//正文编码
            mailMessage.IsBodyHtml = true;//设置为HTML格式
            mailMessage.Priority = MailPriority.Normal;//优先级
            smtpClient.Send(mailMessage);
        }

        /// <summary>
        /// 发送电子邮件
        /// </summary>
        /// <param name="smtpserver">SMTP服务器</param>
        /// <param name="enableSsl">是否启用SSL加密</param>
        /// <param name="userName">登录帐号</param>
        /// <param name="pwd">登录密码</param>
        /// <param name="nickName">发件人昵称</param>
        /// <param name="fromEmail">发件人</param>
        /// <param name="toEmail">收件人</param>
        /// <param name="subj">主题</param>
        /// <param name="bodys">内容</param>
        private static async Task SendMailAsync(string smtpserver, int? prot, bool enableSsl, string userName, string pwd, string nickName, string fromMail, string toMail, string subj, string bodys)
        {
            var smtpClient = new SmtpClient();
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式
            smtpClient.Host = smtpserver;//指定SMTP服务器
            if (prot != null)
            {
                smtpClient.Port = prot.Value;
            }
            smtpClient.Credentials = new NetworkCredential(userName, pwd);//用户名和密码
            smtpClient.EnableSsl = enableSsl;

            MailMessage mailMessage = new MailMessage();

            MailAddress fromAddress = new MailAddress(fromMail, nickName);
            mailMessage.From = fromAddress;

            var toAddresss = toMail.Split(new[] { ',', ';' }).Where(c => !string.IsNullOrEmpty(c)).Select(c => new MailAddress(c)).ToList();
            foreach (var toAddress in toAddresss)
            {
                mailMessage.To.Add(toAddress);
            }

            //mailMessage.Attachments.Add(new Attachment());

            mailMessage.Subject = subj;//主题
            mailMessage.Body = bodys;//内容
            mailMessage.BodyEncoding = Encoding.Default;//正文编码
            mailMessage.IsBodyHtml = true;//设置为HTML格式
            mailMessage.Priority = MailPriority.Normal;//优先级
            await smtpClient.SendMailAsync(mailMessage);
        }

    }

    public class MailServer
    {
        public string DisplayName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public bool IsEnableSSL { get; set; }
        public string FromMail { get; set; }
    }

    public class MailMessages
    {
        public string MailServer { get; set; }
        public string ToMail { get; set; }
        public string Subject { get; set; }
        public string Bodys { get; set; }
        public string FromMail { get; set; }
        public List<MailAttachment> Attachments { get; set; }
    }

    public class MailAttachment
    {
        public Stream Stream { get; set; }

        public string Name { get; set; }

        public string MediaType { get; set; }
    }

    public class MailResultMessaage : MailMessages
    {

        public DateTime SendTime { get; set; }

        public long ElapsedTime { get; set; }

        public int Status { get; set; }

        public string ExceptionInfo { get; set; }


    }


}
