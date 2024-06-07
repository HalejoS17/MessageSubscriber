using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using MailKit.Net.Smtp;
using MimeKit;

class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            channel.QueueDeclare(queue: "email_queue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);

                SendEmail(message);
            };
            channel.BasicConsume(queue: "email_queue",
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }

    private static void SendEmail(string message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("Your Name", "your-email@gmail.com"));
        emailMessage.To.Add(new MailboxAddress("Recipient Name", "recipient-email@gmail.com"));
        emailMessage.Subject = "Test Message";
        emailMessage.Body = new TextPart("plain") { Text = message };

        using (var client = new SmtpClient())
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            client.Connect("smtp.gmail.com", 587, false);
            client.Authenticate("your-email@gmail.com", "your-email-password");
            client.Send(emailMessage);
            client.Disconnect(true);
        }

        Console.WriteLine("Email sent successfully.");
    }
}
