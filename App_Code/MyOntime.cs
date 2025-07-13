using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OnTimeApi;
using System.Security.Cryptography;
using System.IO;
using System.ComponentModel;

/// <summary>
/// Summary description for Ontime
/// </summary>
public class MyOntime
{
    private Settings Settings;
    public OnTime OnTime;
    
    // Initialize object and generate a ticket
    public MyOntime()
	{        
        try
        {
            Settings = new Settings(
                onTimeUrl: "https://mirion.axosoft.com/",
                clientId: "c0c7c0ec-7d0f-4110-aa0f-38484cfcf107",
                clientSecret: "ddc4618b-718f-4477-841d-e9a7c8c4e9f6"
            );

            OnTime = new OnTime(Settings);         
        }
        catch (OnTimeException ex)
        {
            throw ex;
        }
	}
    
}

// This is used to POST an attachment, due to a bug in OnTime that expects the content to be encoded this way
public class UTF8ByteEncoder : ICryptoTransform
{
    public bool CanReuseTransform
    {
        get { return true; }
    }

    public bool CanTransformMultipleBlocks
    {
        get { return true; }
    }

    public int InputBlockSize
    {
        get { return 1; }
    }

    public int OutputBlockSize
    {
        get { return 2; }
    }

    public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
    {
        var originalOutputOffset = outputOffset;
        for (var inputIndex = 0; inputIndex < inputCount; inputIndex++)
        {
            var b = inputBuffer[inputOffset + inputIndex];
            if ((b & 128) > 0)
            {
                outputBuffer[outputOffset++] = (byte)((b >> 6) | 0xc0);
                outputBuffer[outputOffset++] = (byte)((b & 0x3f) | 0x80);
            }
            else
                outputBuffer[outputOffset++] = b;
        }
        return outputOffset - originalOutputOffset;
    }

    public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
    {
        var outputBuffer = new byte[inputBuffer.Length * 2];
        var outputCount = TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, 0);
        return outputBuffer.Take(outputCount).ToArray();
    }

    public void Dispose()
    {
    }
}