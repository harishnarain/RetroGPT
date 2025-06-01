using Microsoft.Extensions.Hosting;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class SerialPortListener : BackgroundService
{
    private readonly RelayService _relayService;
    private SerialPort? _serialPort;

    public SerialPortListener(RelayService relayService)
    {
        _relayService = relayService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            try
            {
                _serialPort = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One)
                {
                    Encoding = Encoding.ASCII,
                    NewLine = "\r",
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };

                _serialPort.Open();
                _serialPort.WriteLine("Welcome to RetroGPT!\r\nType your question:");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var prompt = _serialPort.ReadLine()?.Trim();
                        if (!string.IsNullOrEmpty(prompt))
                        {
                            var response = await _relayService.SendPromptToOpenAI(prompt);
                            _serialPort.WriteLine("\r\n---\r\n" + response + "\r\n---\r\n");
                        }
                    }
                    catch (TimeoutException) { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Serial error: " + ex.Message);
            }
            finally
            {
                _serialPort?.Close();
            }
        });
    }
}