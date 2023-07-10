using System;
using System.Collections;
using System.IO;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;

namespace PKFramework.SerialPort.Scripts
{
    public class SerialThread
    {
        private readonly SerialPortData _portData;
        
        private readonly Queue _inputQueue;
        private readonly Queue _outputQueue;

        private System.IO.Ports.SerialPort _serialPort;

        private bool _stopRequested;

        public Action OnConnected;
        public Action OnDisconnected;

        private bool StopRequested
        {
            get
            {
                lock (this)
                {
                    return _stopRequested;
                }
            }
        }

        public SerialThread(SerialPortData portData)
        {
            _portData = portData;
            _inputQueue = Queue.Synchronized(new Queue());
            _outputQueue = Queue.Synchronized(new Queue());
        }

        [CanBeNull]
        public string ReadSerialMessage()
        {
            if (_inputQueue.Count == 0)
            {
                return null;
            }

            return (string) _inputQueue.Dequeue();
        }

        public void SendSerialMessage(string message)
        {
            _outputQueue.Enqueue(message);
        }

        // ------------------------------------------------------------------------
        // Invoked to indicate to this thread object that it should stop.
        // ------------------------------------------------------------------------
        public void RequestStop()
        {
            lock (this)
            {
                _stopRequested = true;
            }
        }

        public void RunForever()
        {
            // This try is for having a log message in case of an unexpected
            // exception.
            try
            {
                while (!StopRequested)
                {
                    try
                    {
                        // Try to connect
                        AttemptConnection();

                        // Enter the semi-infinite loop of reading/writing to the
                        // device.
                        while (!StopRequested)
                            RunOnce();
                    }
                    catch (Exception ioe)
                    {
                        // A disconnection happened, or there was a problem
                        // reading/writing to the device. Log the detailed message
                        // to the console and notify the listener too.
                        Debug.LogWarning("Exception: " + ioe.Message + " StackTrace: " + ioe.StackTrace);
                        OnDisconnected?.Invoke();

                        // As I don't know in which stage the SerialPort threw the
                        // exception I call this method that is very safe in
                        // disregard of the port's status
                        CloseDevice();

                        // Don't attempt to reconnect just yet, wait some
                        // user-defined time. It is OK to sleep here as this is not
                        // Unity's thread, this doesn't affect frame-rate
                        // throughput.
                        Thread.Sleep(_portData.DelayBeforeReconnecting);
                    }
                }

                // Before closing the COM port, give the opportunity for all messages
                // from the output queue to reach the other endpoint.
                while (_outputQueue.Count != 0)
                {
                    var outputMessage = (string) _outputQueue.Dequeue();
                    _serialPort.Write(outputMessage);
                }

                // Attempt to do a final cleanup. This method doesn't fail even if
                // the port is in an invalid status.
                CloseDevice();
            }
            catch (Exception e)
            {
                Debug.LogError("Unknown exception: " + e.Message + " " + e.StackTrace);
            }
        }

        // ------------------------------------------------------------------------
        // Try to connect to the serial device. May throw IO exceptions.
        // ------------------------------------------------------------------------
        private void AttemptConnection()
        {
            _serialPort = new System.IO.Ports.SerialPort(_portData.Port, _portData.BaudRate, 
                _portData.Parity, _portData.DataBits, _portData.StopBits)
            {
                DtrEnable = _portData.DtrEnable, 
                RtsEnable = _portData.RtsEnable, 
                ReadTimeout = _portData.ReadTimeout, 
                WriteTimeout = _portData.WriteTimeout
            };
            _serialPort.Open();
            OnConnected?.Invoke();
        }

        private void CloseDevice()
        {
            if (_serialPort == null)
            {
                return;
            }

            try
            {
                _serialPort.Close();
            }
            catch (IOException)
            {
                // Nothing to do, not a big deal, don't try to cleanup any further.
            }

            _serialPort = null;
        }
        private void RunOnce()
        {
            try
            {
                // Send a message.
                if (_outputQueue.Count != 0)
                {
                    var outputMessage = (string) _outputQueue.Dequeue();
                    _serialPort.Write(outputMessage);
                }

                // Read a message.
                // If a line was read, and we have not filled our queue, enqueue
                // this line so it eventually reaches the Message Listener.
                // Otherwise, discard the line.
                var inputMessage = _serialPort.ReadLine();
                if (_inputQueue.Count < _portData.MaxUnreadMessages)
                {
                    _inputQueue.Enqueue(inputMessage);
                }
            }
            catch (TimeoutException)
            {
                // This is normal, not everytime we have a report from the serial device
            }
        }
    }


}