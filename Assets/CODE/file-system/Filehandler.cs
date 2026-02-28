using System;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine.Rendering;


struct file_dot_str
{
    public Vector3 color;
    public Vector2 position;
    public Vector2 velocity;
    public float mass;
}

struct lookup_str
{
    public Vector3 color;
    public float mass;
}

public class Filehandler : MonoBehaviour
{
    public bool WRITING;
    public bool READING;
    
    public FPS_TARGETING FPSTARGETER;
    public ComputeShader computeShader;
    public gravity_Csharp gravScript;
    public GameObject progressBar;
    
    GraphicsBuffer _buffer;
    file_dot_str[] _dot;
    lookup_str[] _lookup;
    
    
    public string fileName;
    public string fileDirectory;
    string fileExtension = ".grav";
    string fileFullPath;
    
    private FileStream file;
    private BinaryWriter _writer;
    private BinaryReader _reader;
    
    [Header("File info")] 
    public ushort fps;
    public ushort time;
    public int accuracy = (int)(1 / 0.001f);

    public short rfps;
    public short rtime;
    public int rdotcount;
    
    public uint current_frame = 0;
    private int net_dot;

    [Header("Canvas")]
    public GameObject Draw_canvas;
    public GameObject Generate_canvas;
    public GameObject Replay_canvas;
    
    //rendering
    private Mesh dotMesh;
    private Material dotMaterial;
    GraphicsBuffer dotargsbuffer;
    uint[] dot_args = new uint[4];
    
    /*
    file header:
    2 B: fps 65536
    2 B: time limit ~109 min.
    4 B: dotcount >4B.
    total: 8 B

    lookup:
    3 B: mass
    3 B: color
    total: 6 B

    dot:
    3 B: position x
    3 B: position y
    3 B: velocity x
    3 B: velocity y
    total: 12 B
    */

    #region Set Sim variables
    public void SetFps(string str)
    {
        if (ushort.TryParse(str, out ushort newfps) && newfps != 0)
            fps = newfps;
    }
    public void SetTime(string str)
    {
        if (ushort.TryParse(str, out ushort newtime) && newtime != 0)
            time = newtime;
    }
    #endregion
    

    #region WRITING

    public void Startfile()
    {
        WRITING = true;
        current_frame = 0;
        
        SetCanvas(2);
        
        //sorting out dot buffer
        gravScript.SortingDotBuffer();
        
        net_dot = gravScript.dotCount - gravScript.freeSpace;
        
        progressBar.SetActive(true);
        
        file = File.Create(fileFullPath);
        
        byte[] DataFPS = BitConverter.GetBytes(fps);
        byte[] DataTime = BitConverter.GetBytes(time);
        byte[] DataDot = BitConverter.GetBytes(net_dot);
        
        Array.Resize(ref DataFPS, 2);
        Array.Resize(ref DataTime, 2);
        Array.Resize(ref DataDot, 4);
        
        _writer = new BinaryWriter(file);
        _writer.Write(DataFPS);
        _writer.Write(DataTime);
        _writer.Write(DataDot);
        _writer.Flush();
        
        
        //creating lookup
        _buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.CopyDestination, gravScript.dotCount, sizeof(float) * (3 + 2 + 2 + 1));
        _dot = new file_dot_str[gravScript.dotCount];
        _buffer.SetData(_dot);
        
        Graphics.CopyBuffer(gravScript.DotBuffer, _buffer);
        
        _buffer.GetData(_dot);
        for (int i = 0; i < gravScript.dotCount; i++)
        {
            if (_dot[i].mass == 0) {continue;}
            
            byte[] mass = BitConverter.GetBytes(Mathf.Clamp(Mathf.FloorToInt(_dot[i].mass * 100), 0, 16777215));
            Array.Resize(ref mass, 3);
            // From: 0-1 to 0-255
            byte[] color =
            {
                (byte)Mathf.Clamp(Mathf.FloorToInt(_dot[i].color.x*255), 0, 255), // red
                (byte)Mathf.Clamp(Mathf.FloorToInt(_dot[i].color.y*255), 0, 255), // green
                (byte)Mathf.Clamp(Mathf.FloorToInt(_dot[i].color.z*255), 0, 255)  // blue
            };
            
            _writer.Write(mass);
            _writer.Write(color);
        }
    }
    
    public void Appendfile()
    {
        current_frame++;
        
        _buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.CopyDestination, gravScript.dotCount, sizeof(float) * (3 + 2 + 2 + 1));
        _dot = new file_dot_str[gravScript.dotCount];
        _buffer.SetData(_dot);
        
        Graphics.CopyBuffer(gravScript.DotBuffer, _buffer);
        
        _buffer.GetData(_dot);
        _buffer.Dispose();

        byte[] cahce = new byte[12];
        
        for (int i = 0; i < gravScript.dotCount; i++)
        {
            if (_dot[i].mass == 0) {continue;}
            
            //proccesing data for storage
            Vector2Int pPos = new Vector2Int(Mathf.Clamp(Mathf.FloorToInt(_dot[i].position.x*accuracy)+8388607, 0, 16777216), Mathf.Clamp(Mathf.FloorToInt(_dot[i].position.y*accuracy)+8388607, 0, 16777216));
            byte[] pPosx = BitConverter.GetBytes(pPos.x);
            byte[] pPosy = BitConverter.GetBytes(pPos.y);
            
            Vector2Int pVel = new Vector2Int(Mathf.Clamp(Mathf.FloorToInt(_dot[i].velocity.x*accuracy)+8388607, 0, 16777216), Mathf.Clamp(Mathf.FloorToInt(_dot[i].velocity.y*accuracy)+8388607, 0, 16777216));
            byte[] pVelx = BitConverter.GetBytes(pVel.x);
            byte[] pVely = BitConverter.GetBytes(pVel.y);
            
            cahce[0] = pPosx[0];
            cahce[1] = pPosx[1];
            cahce[2] = pPosx[2];
            cahce[3] = pPosy[0];
            cahce[4] = pPosy[1];
            cahce[5] = pPosy[2];
            cahce[6] = pVelx[0];
            cahce[7] = pVelx[1];
            cahce[8] = pVelx[2];
            cahce[9] = pVely[0];
            cahce[10] = pVely[1];
            cahce[11] = pVely[2];
            
            _writer.Write(cahce);
            _writer.Flush();
        }
    }

    #endregion

    #region READING

    void InitializeReading()
    {
        READING = true;
        current_frame = 0;
        
        SetCanvas(3);
        
        //searching for .grav files
        string[] availableFiles = Directory.GetFiles(fileDirectory);

        for (int i = 0; i < availableFiles.Length; i++)
        {
            string[] parsed = availableFiles[i].Split('.');

            if (parsed[parsed.Length - 1] == "grav")
            {

                file = new FileStream(availableFiles[i], FileMode.Open);

                break;
            }
        }
        
        //initializing reader and gathering data
        _reader = new BinaryReader(file);
        
        rfps = _reader.ReadInt16();
        rtime = _reader.ReadInt16();
        rdotcount = _reader.ReadInt32();

        Debug.Log("rfps: " + rfps.ToString());
        Debug.Log("rtime: " + rtime.ToString());
        Debug.Log("rdotcount: " + rdotcount.ToString());
        
        
        //lookup
        _lookup = new lookup_str[gravScript.dotCount];
        
        for (int i = 0; i < rdotcount; i++)
        {
            if (_dot[i].mass == 0) {continue;}

            byte[] mass_B = _reader.ReadBytes(3);
            Array.Resize(ref mass_B, 4);
            
            float mass_F = (float)BitConverter.ToInt32(mass_B)/100;
            
            Vector3 color_F = new Vector3(
                (float)_reader.ReadBytes(1)[0]/255,
                (float)_reader.ReadBytes(1)[0]/255,
                (float)_reader.ReadBytes(1)[0]/255
            );
            _lookup[i].mass = mass_F;
            _lookup[i].color = color_F;
        }
        
        //rendering

        FPSTARGETER.TARGET_FPS = rfps;
        
        dotMesh = gravScript.DotMesh;
        dotMaterial = gravScript.DotMaterial;
        
        RenderParams rp = new RenderParams(dotMaterial)
        {
            receiveShadows = false,
            shadowCastingMode = ShadowCastingMode.Off
        };

        dot_args[0] = (uint)dotMesh.GetIndexCount(0);
        dot_args[1] = (uint)rdotcount;
        dot_args[2] = (uint)dotMesh.GetIndexStart(0);
        dot_args[3] = (uint)dotMesh.GetBaseVertex(0);
        dotargsbuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, dot_args.Length, dot_args.Length * sizeof(uint));

        dotargsbuffer.SetData(dot_args);

        dotMaterial.renderQueue = 4000;
        
        
        _buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, rdotcount, sizeof(float) * (3 + 2 + 2 + 1));
        
        dotMaterial.SetBuffer("_dotData", _buffer);
    }
    
    public void readfile()
    {
        //setting up a dotbuffer
        _dot = new file_dot_str[rdotcount];
        
        for (int i = 0; i < rdotcount; i++)
        {
            byte[] posx_B = _reader.ReadBytes(3);
            byte[] posy_B = _reader.ReadBytes(3);
            byte[] velx_B = _reader.ReadBytes(3);
            byte[] vely_B = _reader.ReadBytes(3);
                
            Array.Resize(ref posx_B, 4);
            Array.Resize(ref posy_B, 4);
            Array.Resize(ref velx_B, 4);
            Array.Resize(ref vely_B, 4);
               
                
            float posx_float = (float)(BitConverter.ToInt32(posx_B) - 8388607) / accuracy;
            float posy_float = (float)(BitConverter.ToInt32(posy_B) - 8388607) / accuracy;
            float velx_float = (float)(BitConverter.ToInt32(velx_B) - 8388607) / accuracy;
            float vely_float = (float)(BitConverter.ToInt32(vely_B) - 8388607) / accuracy;
            
            
            _dot[i].position = new Vector2(posx_float, posy_float);
            _dot[i].velocity = new Vector2(velx_float, vely_float);
            _dot[i].color = _lookup[i].color;
            _dot[i].mass = _lookup[i].mass;
        }
        
        _buffer.SetData(_dot);
        dotMaterial.SetBuffer("_dotData", _buffer);
        
        Graphics.DrawMeshInstancedIndirect(dotMesh, 0, dotMaterial, new Bounds(Camera.main.transform.position, new Vector3(1,1,1)), dotargsbuffer, layer:10, castShadows:ShadowCastingMode.Off, receiveShadows:false);
        
        current_frame++;
    }
    #endregion
    
    private void Awake()
    {
        fileFullPath = fileDirectory + fileName + fileExtension;
    }
    private void Update()
    {
        //writing
        if (current_frame >= time * fps && WRITING)
        {
            WRITING = false;
            _writer.Flush();
            _writer.Dispose(); 
            //_writer.Close();
            
        }
        if (WRITING) { Appendfile();}
        
        //reading
        if (Input.GetKeyDown(KeyCode.C)) { InitializeReading();}
        if (current_frame >= rtime * rfps && READING)
        {
            READING = false;
            _reader.Dispose(); 
            _reader.Close();

            
        }
        if (READING) {readfile();}
        
    }
    private void OnApplicationQuit()
    {
        _writer.Close();
        File.Delete(fileDirectory + fileName + fileExtension);
    }

    void SetCanvas(int id)
    {
        Draw_canvas.SetActive(false);
        Generate_canvas.SetActive(false);
        Replay_canvas.SetActive(false);
        
        if (id == 1) {Draw_canvas.SetActive(true);}
        else if (id == 2) {Generate_canvas.SetActive(true);}
        else if (id == 3) {Replay_canvas.SetActive(true);}
    }
}
