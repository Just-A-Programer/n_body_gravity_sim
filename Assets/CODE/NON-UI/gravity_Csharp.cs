using System;
using System.Net.NetworkInformation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;


#region STRUCTURES

struct dot_str_Csharp
{
    public Vector3 color;
    public Vector2 position;
    public Vector2 velocity;
    public float mass;
}

struct changeDotsStr_Csharp
{
    public uint changeID;
    public float radius;
    public Vector2 centerPos;
    public float changeDataMass;
    public Vector2 changeDataVel;
    public Vector3 changeDataCol;
}

struct miscellaneousData
{
    public int dotCount;
    public int freeSpace;
}


#endregion


public class gravity_Csharp : MonoBehaviour
{
    #region Viariables
    
    //MODES
    [Header("MODES")]
    public bool RENDER_DOTS;
    public bool COLLISON;
    public bool COMPUTE_SHADER;
    public int  RENDER_DOT_MODE;



    //DOTs
    [Header("DOTs")]
    public int dotCount;
    public float dotMass;

    public float MaxPos;
    public Vector2 ForceVector;

    public int freeSpace;
    
    //File handler
    [Header("FILE HANDLER")]
    public Filehandler fHandler;
    
    //THE COMPUTE SHADER
    [Header("THE COMPUTE SHADER")]
    public ComputeShader computeShader;
    
    //INSTANCING BUFFERS, MESHES & MATERIALS
    [Header("INSTANCING MESHES & MATERIALS")]
    public Mesh DotMesh;
    public Material DotMaterial;

    GraphicsBuffer dotargsbuffer;
    uint[] dot_args = new uint[4];
    


    //COMPUTE BUFFERS
    public GraphicsBuffer DotBuffer;
    GraphicsBuffer ChangeBuffer;
    GraphicsBuffer miscellaneousBuffer;

    GraphicsBuffer DotBuffer_TMP;


    //STRUCTs
    dot_str_Csharp[] results;
    dot_str_Csharp[] Dotinput;
    changeDotsStr_Csharp[] ChangeInput;
    miscellaneousData[] miscellaneousInput;
    


    //KERNEL IDs
    int dot_kernel;
    int change_kernel;
    int CopyBuff_kernel;
    int trajectory_kernel;


    //MOUSE MODE DATA
    [Header("MOUSE MODE DATA")]
    public int mouse_MODE = -1;
    public Color ColorPreset;
    public float MassPresent = 1;
    public Vector2 VelocityPresent;
    public Vector2 PositionPresent;

    public TMPro.TMP_InputField masstext;
    public GameObject Mouse_Shere;
    GameObject Mouse_influence_sphere;
    float Mouse_influence_sphere_radius = 25f;
    int addDotUIClicks = 0;
    public Add_Dot_UI addDotUI;



    //MISCELLANEOUS
    [Header("MISCELLANEOUS")]
    public float G = 1;
    public bool start_empty;
    Camera cam;
    int newDotAmount;
    const int batchSize = 65535;
    
    #endregion

    
    Vector4 floatToVector4(float fl) { return new Vector4(fl, 0f, 0f, 0f); }

    Vector4[] floatArrToVector4(float[] fl)
    {
        Vector4[] arr = new Vector4[fl.Length];
        for (int i = 0; i < fl.Length; i++)
        {
            arr[i] = new Vector4(fl[i], 0f, 0f, 0f);
        }
        return arr;
    }
    
    private Mesh GenerateQuad(float D)
    {
        Mesh m = new Mesh();
        m.vertices = new[] {
            new Vector3(-D/2, -D/2, 0),
            new Vector3( D/2, -D/2, 0),
            new Vector3( D/2,  D/2, 0),
            new Vector3(-D/2,  D/2, 0)
        };
        m.uv = new[] {
            new Vector2(0,0),
            new Vector2(1,0),
            new Vector2(1,1),
            new Vector2(0,1)
        };
        m.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        m.RecalculateBounds();
        m.RecalculateNormals();
        m.RecalculateTangents();
        return m;
    }
    
    
    
    void Start()
    {
        cam = Camera.main;
		

        #region SETTING UP CS BUFFERS
        
        DotBuffer =           new GraphicsBuffer(GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.CopySource, dotCount, sizeof(float) * (3 + 2 + 2 + 1));
		DotBuffer_TMP =       new GraphicsBuffer(GraphicsBuffer.Target.Structured, dotCount, sizeof(float) * (3 + 2 + 2 + 1));
        ChangeBuffer =        new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1,        sizeof(float) * (3 + 2 + 2 + 1 + 1) + sizeof(uint) * 1);
        miscellaneousBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1,        sizeof(int) * 2);

        dot_kernel        = computeShader.FindKernel("CSMain");
        change_kernel     = computeShader.FindKernel("AddorRemoveDots");
        CopyBuff_kernel   = computeShader.FindKernel("CopyBuffer");
        trajectory_kernel = computeShader.FindKernel("CalcTrajectory");


        Dotinput =           new dot_str_Csharp[dotCount];
        ChangeInput =        new changeDotsStr_Csharp[1];
        miscellaneousInput = new miscellaneousData[1];
        
        
        #endregion

        #region INITIALIZING DATA


        //constants
        computeShader.SetFloat("G", G);
        

        //dot data
        ForceVector *= -1;
        if (!start_empty)
        {
            for (int i = 0; i < dotCount; i++)
            {
                Vector2 polarcord = new Vector2(UnityEngine.Random.Range(0, MaxPos), UnityEngine.Random.Range(0, 2 * Mathf.PI));

                Vector2 newpos = new Vector2(polarcord.x * Mathf.Cos(polarcord.y), polarcord.x * Mathf.Sin(polarcord.y));

                float2x2 Forcematrix = new float2x2
                    (
                    new Vector2(Mathf.Cos(math.atan2(newpos.y, newpos.x)), -Mathf.Sin(math.atan2(newpos.y, newpos.x))),
                    new Vector2(Mathf.Sin(math.atan2(newpos.y, newpos.x)), Mathf.Cos(math.atan2(newpos.y, newpos.x)))
                    );

                Dotinput[i].color = new Vector3(ColorPreset.r, ColorPreset.g, ColorPreset.b);
                Dotinput[i].position = newpos;
                Dotinput[i].velocity = math.mul(new Vector2(1, 1) * Mathf.Sqrt((G) / Vector2.Distance(new Vector2(), newpos)), Forcematrix);
                Dotinput[i].mass = dotMass;
            }
        }
        else
        {
            freeSpace = dotCount;
        }
        
        //change input
        ChangeInput[0].changeID = 69;
        ChangeInput[0].radius = 50f;
        ChangeInput[0].centerPos = new Vector2(0, 0);
        ChangeInput[0].changeDataMass = 0;
        ChangeInput[0].changeDataVel = new Vector2(0, 0);
        ChangeInput[0].changeDataCol = new Vector3(0, 0, 0);
        
        
        //miscellaneous input
        miscellaneousInput[0].dotCount = dotCount;
        miscellaneousInput[0].freeSpace = freeSpace;

        #endregion

        #region BINDING BUFFERS WITH CS


        DotBuffer.SetData(Dotinput);
        ChangeBuffer.SetData(ChangeInput);
        miscellaneousBuffer.SetData(miscellaneousInput);
        
        
        RebindGPUBuffers(new bool[5] { true, true, true, false, false });

        #endregion

        #region INSTANCING SETUP

        //### indirect instancing setup ###

        // dot renderer

        RenderParams rp = new RenderParams(DotMaterial)
        {
            receiveShadows = false,
            shadowCastingMode = ShadowCastingMode.Off
        };

        dot_args[0] = (uint)DotMesh.GetIndexCount(0);
        dot_args[1] = (uint)dotCount;
        dot_args[2] = (uint)DotMesh.GetIndexStart(0);
        dot_args[3] = (uint)DotMesh.GetBaseVertex(0);
        dotargsbuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, dot_args.Length, dot_args.Length * sizeof(uint));

        dotargsbuffer.SetData(dot_args);

        DotMaterial.renderQueue = 4000;
        
        DotMaterial.SetBuffer("_dotData", DotBuffer);
        

        #endregion
    }

    private void FixedUpdate()
    {

    }

    private void Update()
    { 
        if (fHandler.WRITING)
        {
            computeShader.SetVector("fixedDeltaTime", floatToVector4(1f/fHandler.fps));
            computeShader.SetVector("COLLISION", floatToVector4((float)Convert.ToInt32(COLLISON)));


            if (COMPUTE_SHADER)
            {
                for (int i = 0; i < (float)dotCount / (float)batchSize; i++)
                {
                    computeShader.SetInt("DispatchOffset", i * batchSize);
                    computeShader.Dispatch(dot_kernel, (int)MathF.Min(dotCount - i * batchSize, batchSize), 1, 1);
                }
            }
        }
        
        
        // rendering obj
        if (RENDER_DOTS)
        {
            Graphics.DrawMeshInstancedIndirect(DotMesh, 0, DotMaterial, new Bounds(cam.transform.position, new Vector3(1,1,1)), dotargsbuffer, layer:10, castShadows:ShadowCastingMode.Off, receiveShadows:false);
        }
        
        #region mousemode

        Vector3 point = new Vector3();
        Vector2 mousePos = new Vector2();
        if (mouse_MODE != -1)
        {
            mousePos = Input.mousePosition;
            point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, cam.nearClipPlane));

            if (!(Input.GetKey(KeyCode.LeftControl)))
                Mouse_influence_sphere_radius += Mouse.current.scroll.ReadValue().y * 1f;

            if (Mouse_influence_sphere_radius < 1)
                Mouse_influence_sphere_radius = 1;

            Mouse_influence_sphere.transform.position = new Vector3(point.x, point.y, 0);
            Mouse_influence_sphere.transform.localScale = new Vector3(Mouse_influence_sphere_radius * 2, Mouse_influence_sphere_radius * 2, 0);
        }

        // add dots
        if (mouse_MODE == 0)
        {
            float r = Mouse_influence_sphere_radius;

            newDotAmount = Sigma_func(1, (int)Mathf.Floor(r), f => (int)Mathf.Floor((2 * MathF.PI) / (2 * (float)Math.Atan(0.5f / f))));

            computeShader.SetInt("newdotamount", newDotAmount);
            ChangeInput = new changeDotsStr_Csharp[1];
            ChangeInput[0].changeID = 0;
            ChangeInput[0].radius = r;
            ChangeInput[0].centerPos = PositionPresent;//new Vector2(point.x, point.y);

            ChangeInput[0].changeDataCol = new Vector3(ColorPreset.r, ColorPreset.g, ColorPreset.b);
            ChangeInput[0].changeDataMass = MassPresent;
            ChangeInput[0].changeDataVel = VelocityPresent;


            ChangeBuffer.SetData(ChangeInput);

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                addDotUIClicks++;
                if (addDotUIClicks == 3)
                {
                    addDotUIClicks = 0;
                    if (newDotAmount >= freeSpace)
                    {
                        DotBuffer_TMP.Dispose();
                        DotBuffer_TMP = new GraphicsBuffer(GraphicsBuffer.Target.Structured, dotCount, sizeof(float) * (3 + 2 + 2 + 1));
                        computeShader.SetBuffer(CopyBuff_kernel, "inputData_TMP", DotBuffer_TMP);
                        computeShader.SetInt("CopyID", 1);
                        
                        computeShader.Dispatch(CopyBuff_kernel, 1, 1, 1);

                        dotCount = (int)MathF.Floor(1.25f*(dotCount + newDotAmount - freeSpace));
                        freeSpace = 0;
                        
                        DotBuffer.Dispose();
                        DotBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.CopySource, dotCount, sizeof(float) * (3 + 2 + 2 + 1));
            

                        // IMPORTANT: bind NEW buffer as destination
                        computeShader.SetBuffer(CopyBuff_kernel, "inputData", DotBuffer);
                        //computeShader.SetBuffer(CopyBuff1_kernel, "inputData_TMP", DotBuffer_TMP);

                        // Rebind other kernels
                        computeShader.SetBuffer(dot_kernel, "inputData", DotBuffer);
                        computeShader.SetBuffer(change_kernel, "inputData", DotBuffer);
                        DotMaterial.SetBuffer("_dotData", DotBuffer);

                        // Tell copy shader direction
                        computeShader.SetInt("CopyID", 0);

                        computeShader.Dispatch(CopyBuff_kernel, 1, 1, 1);


                        computeShader.Dispatch(change_kernel, 1, 1, 1);
            
                        miscellaneousInput[0].dotCount = dotCount;
                        miscellaneousInput[0].freeSpace = freeSpace;
                        miscellaneousBuffer.SetData(miscellaneousInput);

            
                        //RebindGPUBuffers(new bool[5] {true, true, true, true, true});
                        dotargsbuffer.Release();
                        dot_args[0] = (uint)DotMesh.GetIndexCount(0);
                        dot_args[1] = (uint)dotCount;
                        dot_args[2] = (uint)DotMesh.GetIndexStart(0);
                        dot_args[3] = (uint)DotMesh.GetBaseVertex(0);
                        dotargsbuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, dot_args.Length, dot_args.Length * sizeof(uint));
                        dotargsbuffer.SetData(dot_args);

                        DotMaterial.SetBuffer("_dotData", DotBuffer);
                        
                    }
                    else
                    {

                        computeShader.Dispatch(change_kernel, 1, 1, 1);
                    }
                }
            }

            if (addDotUIClicks == 1)
            {
                addDotUIClicks++;
                addDotUI.firstpass();
            }

        }
        // remove dots
        else if (mouse_MODE == 1) 
        {

            ChangeInput = new changeDotsStr_Csharp[1];
            ChangeInput[0].changeID = 1;
            ChangeInput[0].radius = Mouse_influence_sphere_radius;
            ChangeInput[0].centerPos = new Vector2(point.x, point.y);
            ChangeBuffer.SetData(ChangeInput);

            if (Input.GetKey(KeyCode.Mouse0))
            {
                
                computeShader.Dispatch(change_kernel, 1, 1, 1);
            }
        }

        //change color
        else if (mouse_MODE == 2)
        {

            ChangeInput = new changeDotsStr_Csharp[1];
            ChangeInput[0].changeID = 2;
            ChangeInput[0].radius = Mouse_influence_sphere_radius;
            ChangeInput[0].centerPos = new Vector2(point.x, point.y);

            ChangeInput[0].changeDataCol = new Vector3(ColorPreset.r, ColorPreset.g, ColorPreset.b);

            ChangeBuffer.SetData(ChangeInput);

            if (Input.GetKey(KeyCode.Mouse0))
            {

                computeShader.Dispatch(change_kernel, 1, 1, 1);
            }
        }

        //change mass
        else if (mouse_MODE == 3)
        {

            ChangeInput = new changeDotsStr_Csharp[1];
            ChangeInput[0].changeID = 3;
            ChangeInput[0].radius = Mouse_influence_sphere_radius;
            ChangeInput[0].centerPos = new Vector2(point.x, point.y);

            ChangeInput[0].changeDataMass = MassPresent;

            ChangeBuffer.SetData(ChangeInput);

            if (Input.GetKey(KeyCode.Mouse0))
            {

                computeShader.Dispatch(change_kernel, 1, 1, 1);
            }
        }

        // clears mouse mode
        if (Input.GetKeyDown(KeyCode.Mouse1) && mouse_MODE != -1)
        {
            mouse_MODE = -1;
            addDotUIClicks = 0;
            GameObject.Destroy(Mouse_influence_sphere);
        }

        #endregion

    }
    

    void RebindGPUBuffers(bool[] exc)
    {
        
        //GRAPHICS BUFFERS
        if (exc[0])
        {
            
            computeShader.SetBuffer(dot_kernel, "inputData", DotBuffer);
            computeShader.SetBuffer(dot_kernel, "miscData",  miscellaneousBuffer);
            
            computeShader.SetBuffer(trajectory_kernel, "inputData", DotBuffer);
            computeShader.SetBuffer(trajectory_kernel, "changeDots",  ChangeBuffer);
            computeShader.SetBuffer(trajectory_kernel, "miscData",  miscellaneousBuffer);

        }

        if (exc[1])
        {
            computeShader.SetBuffer(change_kernel, "inputData", DotBuffer);
            computeShader.SetBuffer(change_kernel, "miscData", miscellaneousBuffer);
            computeShader.SetBuffer(change_kernel, "changeDots", ChangeBuffer);
        }

        if (exc[2])
        {
            computeShader.SetBuffer(CopyBuff_kernel, "inputData", DotBuffer);
            computeShader.SetBuffer(CopyBuff_kernel, "inputData_TMP", DotBuffer_TMP);
        }

        //RENDER BUFFERS
        if (exc[3])
        {
            dot_args[0] = (uint)DotMesh.GetIndexCount(0);
            dot_args[1] = (uint)dotCount;
            dot_args[2] = (uint)DotMesh.GetIndexStart(0);
            dot_args[3] = (uint)DotMesh.GetBaseVertex(0);
            dotargsbuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, dot_args.Length, dot_args.Length * sizeof(uint));
            dotargsbuffer.SetData(dot_args);
        }

        if (exc[4])
        {
            DotMaterial.SetBuffer("_dotData", DotBuffer);
        }
    }

    int Sigma_func(int start, int end, Func<int, int> f)
    {
        int sum = 0;

        for (; start <= end; start++)
        {
            sum += f(start);
        }

        return sum;
    }



    #region Button functions


    public void ChangeMouseMode(int id)
    {
        mouse_MODE = id;
        if (Mouse_influence_sphere == null)
            Mouse_influence_sphere = GameObject.Instantiate(Mouse_Shere);
    }
    public void ChangeMass(string str)
    {
        if (float.TryParse(str, out float newMass) && newMass != 0)
            MassPresent = newMass;
        masstext.text = "";
    }

    public void ChangeColorPreset(string str)
    {
        if (str == "RED")
            ColorPreset = new Color(1, 0, 0);
        else if (str == "BLUE")
            ColorPreset = new Color(0, 0, 1);
        else if (str == "GREEN")
            ColorPreset = new Color(0, 1, 0);
        else if (str == "PURPLE")
            ColorPreset = new Color(1, 0, 1);
        else if (str == "CYAN")
            ColorPreset = new Color(0, 1, 1);
        else if (str == "WHITE")
            ColorPreset = new Color(1, 1, 1);
    }

    public void ChangeRenderMode(int id)
    {
        RENDER_DOT_MODE = id;
        DotMaterial.SetInt("_RENDER_DOT_MODE", id);
    }

    public void WipeAllDots()
    {
        DotBuffer.Release();
        DotBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, dotCount, sizeof(float) * (3 + 2 + 2 + 1));
        DotBuffer.SetData(new dot_str_Csharp[dotCount]);
        miscellaneousInput = new miscellaneousData[1];
        miscellaneousInput[0].dotCount = dotCount;
        miscellaneousInput[0].freeSpace = dotCount;
        miscellaneousBuffer.SetData(miscellaneousInput);
        freeSpace = dotCount;
        RebindGPUBuffers(new bool[5] {true, true, true, true, true});
    }

    #endregion

    private void OnApplicationQuit()
    {
        DotBuffer.Release();
        ChangeBuffer.Release();
        miscellaneousBuffer.Release();
        dotargsbuffer.Release();
        
    }
}
