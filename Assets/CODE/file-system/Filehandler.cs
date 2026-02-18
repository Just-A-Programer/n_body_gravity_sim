using System;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

public class Filehandler : MonoBehaviour
{
    public string fileName;
    public string fileDirectory;
    public string fileExtension;
    string fileFullPath;
    
    private FileStream file;
    private BinaryWriter _writer;
    
    [Header("File info")] 
    public ushort fps;
    public ushort time;
    public uint dotcount_tmp;


    public void WriteByteArr(FileStream file, byte[] byteArr)
    {
        file.Write(byteArr, 0, byteArr.Length);
    }
    
    public void Startfile(uint dotcount)
    {
        file = File.Create(fileFullPath);

        /*
         
         file header:
        2 B: fps 65536
        2 B: time limit ~109 min.
        4 B: dotcount >4B.
        total: 16 hex, 8 B
        
        
        dot:
        3 B: x
        3 B: y
        2 B: color
        total: 16 hex, 8 B
        
        */
        
        byte[] DataFPS = BitConverter.GetBytes(fps);
        byte[] DataTime = BitConverter.GetBytes(time);
        byte[] DataDot = BitConverter.GetBytes(dotcount);
        
        Array.Resize(ref DataFPS, 2);
        Array.Resize(ref DataTime, 2);
        Array.Resize(ref DataDot, 4);
        
        _writer = new BinaryWriter(file);
        _writer.Write(DataFPS);
        _writer.Write(DataTime);
        _writer.Write(DataDot);
        _writer.Flush();
    }
    
    public void Appendfile()
    {
        Debug.Log("Appended");
        
        
        _writer.Write(0x55);
        _writer.Flush();
        
    }
    
    
    
    private void Awake()
    {
        fileFullPath = fileDirectory + fileName + fileExtension;
        
        Startfile(dotcount_tmp);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) { Appendfile();}
    }
    private void OnApplicationQuit()
    {
        _writer.Flush();
        _writer.Close();
        File.Delete(fileDirectory + fileName + fileExtension);
    }
}
