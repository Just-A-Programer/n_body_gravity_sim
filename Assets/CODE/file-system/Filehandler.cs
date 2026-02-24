using System;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;


struct file_dot_str
{
    public Vector3 color;
    public Vector2 position;
    public Vector2 velocity;
    public float mass;
}


public class Filehandler : MonoBehaviour
{
    public ComputeShader computeShader;
    public gravity_Csharp gravScript;
    
    GraphicsBuffer _buffer;
    file_dot_str[] _dot;
    
    public string fileName;
    public string fileDirectory;
    public string fileExtension;
    string fileFullPath;
    
    private FileStream file;
    private BinaryWriter _writer;
    private BinaryReader _reader;
    
    [Header("File info")] 
    public ushort fps;
    public ushort time;
    public uint dotcount_tmp;
    public int accuracy = (int)(1 / 0.001f);

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
    
    ushort Pack(int a, int b, int c)
    {
        return (ushort)(
            ((a & 0x1F) << 11) |
            ((b & 0x1F) << 6)  |
            ((c & 0x1F) << 1)
        );
    }
    Vector3Int Unpack(ushort packed)
    {
        int a = (packed >> 11) & 0x1F;
        int b = (packed >> 6)  & 0x1F;
        int c = (packed >> 1)  & 0x1F;

        return new Vector3Int(a, b, c);
    }
    
    public void Startfile(uint dotcount)
    {
        file = File.Create(fileFullPath);
        
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
        
        Graphics.CopyBuffer(gravScript.DotBuffer, _buffer);
        
        _buffer.GetData(_dot);

        byte[] cahce = new byte[8];
        
        for (int i = 0; i < gravScript.dotCount; i++)
        {
            if (_dot[i].mass == 0) {continue;}
            
            //proccesing data for storage
            Vector2Int pPos = new Vector2Int(Mathf.Clamp(Mathf.FloorToInt(_dot[i].position.x*accuracy)+8388607, 0, 16777216), Mathf.Clamp(Mathf.FloorToInt(_dot[i].position.y*accuracy)+8388607, 0, 16777216));
            byte[] pPosx = BitConverter.GetBytes(pPos.x);
            byte[] pPosy = BitConverter.GetBytes(pPos.y);
            
            
            int[] pColor_raw =
            {
                Mathf.Clamp(Mathf.FloorToInt(_dot[i].color.x*31), 0, 31), // red
                Mathf.Clamp(Mathf.FloorToInt(_dot[i].color.y*31), 0, 31), // green
                Mathf.Clamp(Mathf.FloorToInt(_dot[i].color.z*31), 0, 31)  // blue
            };
            byte[] pColor = BitConverter.GetBytes(Pack(pColor_raw[0], pColor_raw[1], pColor_raw[2]));
            /*{
                (byte)(pColor_raw[0] + Mathf.FloorToInt(pColor_raw[1]/10)),
                (byte)((pColor_raw[1] % 10) + pColor_raw[2])
            };*/
            
            cahce[0] = pPosx[0];
            cahce[1] = pPosx[1];
            cahce[2] = pPosx[2];
            cahce[3] = pPosy[0];
            cahce[4] = pPosy[1];
            cahce[5] = pPosy[2];
            cahce[6] = pColor[0];
            cahce[7] = pColor[1];
            
            _writer.Write(cahce);
            _writer.Flush();
        }
    }

    public void readfile()
    {
        _writer.Seek(0, SeekOrigin.Begin);
        
        _reader = new BinaryReader(file);
        
        short rfps = _reader.ReadInt16();
        short rtime = _reader.ReadInt16();
        int rdotcount = _reader.ReadInt32();

        Debug.Log("rfps: " + rfps.ToString());
        Debug.Log("rtime: " + rtime.ToString());
        Debug.Log("rdotcount: " + rdotcount.ToString());

        for (int o = 0; o < 5/*rfps * rtime*/; o++)
        {
            for (int i = 0; i < rdotcount; i++)
            {
                byte[] posx_B = _reader.ReadBytes(3);
                byte[] posy_B = _reader.ReadBytes(3);
                byte[] coll_B = _reader.ReadBytes(2);
                
                Array.Resize(ref posx_B, 4);
                Array.Resize(ref posy_B, 4);
                ushort coll_short = BitConverter.ToUInt16(coll_B);
                
                
                float posx_float = (float)(BitConverter.ToInt32(posx_B) - 8388607) / accuracy;
                float posy_float = (float)(BitConverter.ToInt32(posy_B) - 8388607) / accuracy;


                Vector3Int coll_unpacked = Unpack(coll_short);
                Vector3Int coll_float = new Vector3Int(
                    Mathf.FloorToInt(((float)coll_unpacked.x / 31) * 255),
                    Mathf.FloorToInt(((float)coll_unpacked.y / 31) * 255),
                    Mathf.FloorToInt(((float)coll_unpacked.z / 31) * 255)
                    );
                
                Debug.Log("");
                Debug.Log("frame: " + o.ToString() + " object: " + i.ToString());
                Debug.Log("   posx: " + posx_float + " posy: " + posy_float);
                Debug.Log("   r: " + coll_float.x + " g: " + coll_float.y + " b: " + coll_float.z);
            }
        }

        _reader.Close();
    }
    
    private void Awake()
    {
        fileFullPath = fileDirectory + fileName + fileExtension;

        _buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.CopyDestination, gravScript.dotCount, sizeof(float) * (3 + 2 + 2 + 1));
        _dot = new file_dot_str[gravScript.dotCount];
        _buffer.SetData(_dot);
        
        Startfile(dotcount_tmp);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) { Appendfile();}
        if (Input.GetKeyDown(KeyCode.C)) { readfile();}
    }
    private void OnApplicationQuit()
    {
        _writer.Dispose();
        _writer.Close();
        File.Delete(fileDirectory + fileName + fileExtension);
    }
}
