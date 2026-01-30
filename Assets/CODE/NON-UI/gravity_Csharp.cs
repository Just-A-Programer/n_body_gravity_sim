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

struct Grid_str_Csharp
{
    public Vector2 position;
    public Vector2Int MassTPossition;
    public int mass;
    public int jailerID;

    /*
    Grid Sizes (6):
    0: 0.5x0.5
    1: 1x1     (x2)
    2: 2x2     (x2)
    3: 4x4     (x2)
    4: 8x8     (x2)
    5: 16x16   (x2)
    */

}

unsafe struct GridParams_str
{
    public float GridSideLenght;
    public fixed float GridCellLenght[6];
    public fixed float GridTotalCellCount[6];
    public fixed float GridSideCellCount[6];
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
    [Range(0, 6)]
    public int  RENDER_GRID;
    public bool COMPUTE_SHADER;
    public int  RENDER_DOT_MODE;



    //DOTs
    [Header("DOTs")]
    public int dotCount;
    public float dotMass;

    public float MaxPos;
    public Vector2 ForceVector;

    public int freeSpace;

    //THE GRIDS
    public Vector2 GridstartingPoss;
    private static float   GridSideLenght =   1024;
    private static float[] GridCellLenght = { 0.5f, 1.0f, 2.0f, 4.0f, 8.0f, 16.0f };
    private float[] GridTotalCellCount    =   new float[6];
    private float[] GridSideCellCount     =   new float[6];
    
    //THE COMPUTE SHADER
    [Header("THE COMPUTE SHADER")]
    public ComputeShader computeShader;



    //INSTANCING BUFFERS, MESHES & MATERIALS
    [Header("INSTANCING MESHES & MATERIALS")]
    public Mesh DotMesh;
    public Material DotMaterial;

    Mesh[] GridMesh = new Mesh[6];
    public Material[] GridMaterial = new Material[6];

    GraphicsBuffer dotargsbuffer;
    uint[] dot_args = new uint[4];

    GraphicsBuffer[] Gridargsbuffer = new GraphicsBuffer[6];
    uint[] Grid0_args = new uint[5];
    uint[] Grid1_args = new uint[5];
    uint[] Grid2_args = new uint[5];
    uint[] Grid3_args = new uint[5];
    uint[] Grid4_args = new uint[5];
    uint[] Grid5_args = new uint[5];


    //COMPUTE BUFFERS
    GraphicsBuffer GridParams;
    GraphicsBuffer GridBuffer0;
    GraphicsBuffer GridBuffer1;
    GraphicsBuffer GridBuffer2;
    GraphicsBuffer GridBuffer3;
    GraphicsBuffer GridBuffer4;
    GraphicsBuffer GridBuffer5;

    GraphicsBuffer DotBuffer;
    GraphicsBuffer ChangeBuffer;
    GraphicsBuffer miscellaneousBuffer;

    GraphicsBuffer DotBuffer_TMP;



    //STRUCTs
    dot_str_Csharp[] results;
    dot_str_Csharp[] Dotinput;
    changeDotsStr_Csharp[] ChangeInput;
    miscellaneousData[] miscellaneousInput;

    GridParams_str[] GridParamsInput;
    Grid_str_Csharp[] Grid0Input;
    Grid_str_Csharp[] Grid1Input;
    Grid_str_Csharp[] Grid2Input;
    Grid_str_Csharp[] Grid3Input;
    Grid_str_Csharp[] Grid4Input;
    Grid_str_Csharp[] Grid5Input;


    //KERNEL IDs
    int Wipe0_kernel;
    int Wipe1_kernel;
    int Wipe2_kernel;
    int Wipe3_kernel;
    int Wipe4_kernel;
    int Wipe5_kernel;

    int UptGrid0_kernel;
    int UptGrid1_kernel;
    int UptGrid2_kernel;
    int UptGrid3_kernel;
    int UptGrid4_kernel;
    int UptGrid5_kernel;
    
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
    string next_frame_id = "";
    int newDotAmount;
    bool resizing = false;
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
    
    private void Start()
    {
        cam = Camera.main;
        
        GridSideCellCount[0]  = GridSideLenght / GridCellLenght[0];
        GridSideCellCount[1]  = GridSideLenght / GridCellLenght[1];
        GridSideCellCount[2]  = GridSideLenght / GridCellLenght[2];
        GridSideCellCount[3]  = GridSideLenght / GridCellLenght[3];
        GridSideCellCount[4]  = GridSideLenght / GridCellLenght[4];
        GridSideCellCount[5]  = GridSideLenght / GridCellLenght[5];
        
        GridTotalCellCount[0] = Mathf.Pow( GridSideCellCount[0], 2 );
        GridTotalCellCount[1] = Mathf.Pow( GridSideCellCount[1], 2 );
        GridTotalCellCount[2] = Mathf.Pow( GridSideCellCount[2], 2 );
        GridTotalCellCount[3] = Mathf.Pow( GridSideCellCount[3], 2 );
        GridTotalCellCount[4] = Mathf.Pow( GridSideCellCount[4], 2 );
        GridTotalCellCount[5] = Mathf.Pow( GridSideCellCount[5], 2 );

        for (int i = 0; i < 6; i++)
        {
            Debug.Log(i.ToString() + ": " + GridCellLenght[i].ToString() + "   " + GridSideCellCount[i].ToString() + "   " + GridTotalCellCount[i].ToString());
        }
        
        #region SETTING UP CS BUFFERS
        
        DotBuffer =           new GraphicsBuffer(GraphicsBuffer.Target.Structured, dotCount, sizeof(float) * (3 + 2 + 2 + 1));
        ChangeBuffer =        new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1,        sizeof(float) * (3 + 2 + 2 + 1 + 1) + sizeof(uint) * 1);
        miscellaneousBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1,        sizeof(int) * 2);
        
        GridParams  = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, sizeof(float) * (1 + ( 3 * 6 )));
        GridBuffer0 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)GridTotalCellCount[0], sizeof(float) * (2) + sizeof(int) * (2 + 1 + 1));
        GridBuffer1 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)GridTotalCellCount[1], sizeof(float) * (2) + sizeof(int) * (2 + 1 + 1));
        GridBuffer2 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)GridTotalCellCount[2], sizeof(float) * (2) + sizeof(int) * (2 + 1 + 1));
        GridBuffer3 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)GridTotalCellCount[3], sizeof(float) * (2) + sizeof(int) * (2 + 1 + 1));
        GridBuffer4 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)GridTotalCellCount[4], sizeof(float) * (2) + sizeof(int) * (2 + 1 + 1));
        GridBuffer5 = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)GridTotalCellCount[5], sizeof(float) * (2) + sizeof(int) * (2 + 1 + 1));

        DotBuffer_TMP = new GraphicsBuffer(GraphicsBuffer.Target.Structured, dotCount, sizeof(float) * (3 + 2 + 2 + 1));

        
    
    
        Wipe0_kernel      = computeShader.FindKernel("WipeGrid0");
        Wipe1_kernel      = computeShader.FindKernel("WipeGrid1");
        Wipe2_kernel      = computeShader.FindKernel("WipeGrid2");
        Wipe3_kernel      = computeShader.FindKernel("WipeGrid3");
        Wipe4_kernel      = computeShader.FindKernel("WipeGrid4");
        Wipe5_kernel      = computeShader.FindKernel("WipeGrid5");
        
        UptGrid0_kernel   = computeShader.FindKernel("UpdateGrid0");
        UptGrid1_kernel   = computeShader.FindKernel("UpdateGrid1");
        UptGrid2_kernel   = computeShader.FindKernel("UpdateGrid2");
        UptGrid3_kernel   = computeShader.FindKernel("UpdateGrid3");
        UptGrid4_kernel   = computeShader.FindKernel("UpdateGrid4");
        UptGrid5_kernel   = computeShader.FindKernel("UpdateGrid5");

        dot_kernel        = computeShader.FindKernel("CSMain");
        change_kernel     = computeShader.FindKernel("AddorRemoveDots");
        CopyBuff_kernel   = computeShader.FindKernel("CopyBuffer");
        trajectory_kernel = computeShader.FindKernel("CalcTrajectory");


        Dotinput =           new dot_str_Csharp[dotCount];
        ChangeInput =        new changeDotsStr_Csharp[1];
        miscellaneousInput = new miscellaneousData[1];

        GridParamsInput = new GridParams_str[ 1 ];
        Grid0Input = new Grid_str_Csharp[ (int)GridTotalCellCount[0] ];
        Grid1Input = new Grid_str_Csharp[ (int)GridTotalCellCount[1] ];
        Grid2Input = new Grid_str_Csharp[ (int)GridTotalCellCount[2] ];
        Grid3Input = new Grid_str_Csharp[ (int)GridTotalCellCount[3] ];
        Grid4Input = new Grid_str_Csharp[ (int)GridTotalCellCount[4] ];
        Grid5Input = new Grid_str_Csharp[ (int)GridTotalCellCount[5] ];
        
        
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
        
        //GridParams
        unsafe
        {
            fixed (GridParams_str* gp = &GridParamsInput[0])
            {
                for (int i = 0; i < 5; i++)
                {
                    gp->GridSideLenght         = GridSideLenght;
                    gp->GridCellLenght[i]      = GridCellLenght[i];
                    gp->GridSideCellCount[i]   = GridSideCellCount[i];
                    gp->GridTotalCellCount[i]  = GridTotalCellCount[i];
                }
            }
            //GridParamsInput[0].GridCellLenght = GridCellLenght;
            //GridParamsInput[0].GridSideCellCount = GridSideCellCount;
            //GridParamsInput[0].GridTotalCellCount = GridTotalCellCount;
        }
        
        //Grid input
        //grid0
        for (int i = 0; i < GridTotalCellCount[0]; i++)
        {
            Grid0Input[i].position = GridstartingPoss;
            Grid0Input[i].jailerID = -1;
        }
        //grid1
        for (int i = 0; i < GridTotalCellCount[1]; i++)
        {
            Grid1Input[i].position = GridstartingPoss;
        }
        //grid2
        for (int i = 0; i < GridTotalCellCount[2]; i++)
        {
            Grid2Input[i].position = GridstartingPoss;
        }
        //grid3
        for (int i = 0; i < GridTotalCellCount[3]; i++)
        {
            Grid3Input[i].position = GridstartingPoss;
        }
        //grid4
        for (int i = 0; i < GridTotalCellCount[4]; i++)
        {
            Grid4Input[i].position = GridstartingPoss;
        }
        //grid5
        for (int i = 0; i < GridTotalCellCount[5]; i++)
        {
            Grid5Input[i].position = GridstartingPoss;
        }
        
        //miscellaneous input
        miscellaneousInput[0].dotCount = dotCount;
        miscellaneousInput[0].freeSpace = freeSpace;

        #endregion

        #region BINDING BUFFERS WITH CS


        DotBuffer.SetData(Dotinput);
        ChangeBuffer.SetData(ChangeInput);
        miscellaneousBuffer.SetData(miscellaneousInput);

        unsafe
        {
            Debug.Log(GridParamsInput[0].GridCellLenght[0].ToString());
            Debug.Log(GridParamsInput[0].GridTotalCellCount[0].ToString());
        }
        
        GridParams.SetData(GridParamsInput);
        GridBuffer0.SetData(Grid0Input);
        GridBuffer1.SetData(Grid1Input);
        GridBuffer2.SetData(Grid2Input);
        GridBuffer3.SetData(Grid3Input);
        GridBuffer4.SetData(Grid4Input);
        GridBuffer5.SetData(Grid5Input);
        
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


        // Grid renderer
        GridMesh[0] = GenerateQuad( GridCellLenght[0] );
        GridMesh[1] = GenerateQuad( GridCellLenght[1] );
        GridMesh[2] = GenerateQuad( GridCellLenght[2] );
        GridMesh[3] = GenerateQuad( GridCellLenght[3] );
        GridMesh[4] = GenerateQuad( GridCellLenght[4] );
        GridMesh[5] = GenerateQuad( GridCellLenght[5] );
        
        //grid0
        RenderParams rp_grid0 = new RenderParams(GridMaterial[0])
        {
            receiveShadows = false,
            shadowCastingMode = ShadowCastingMode.Off
        };

        Grid0_args[0] = (uint)GridMesh[0].GetIndexCount(0);
        Grid0_args[1] = (uint)GridTotalCellCount[0];
        Grid0_args[2] = (uint)GridMesh[0].GetIndexStart(0);
        Grid0_args[3] = (uint)GridMesh[0].GetBaseVertex(0);
        Grid0_args[4] = (uint)0;
        Gridargsbuffer[0] = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, Grid0_args.Length, Grid0_args.Length * sizeof(uint));
        Gridargsbuffer[0].SetData(Grid0_args);

        GridMaterial[0].renderQueue = 2000;
        
        GridMaterial[0].SetBuffer("GridBuff", GridBuffer0);
        GridMaterial[0].SetFloat("Level", 0);
        
        //grid1
        RenderParams rp_grid1 = new RenderParams(GridMaterial[1])
        {
            receiveShadows = false,
            shadowCastingMode = ShadowCastingMode.Off
        };
        
        Grid1_args[0] = (uint)GridMesh[1].GetIndexCount(0);
        Grid1_args[1] = (uint)GridTotalCellCount[1];
        Grid1_args[2] = (uint)GridMesh[1].GetIndexStart(0);
        Grid1_args[3] = (uint)GridMesh[1].GetBaseVertex(0);
        Grid1_args[4] = (uint)0;
        Gridargsbuffer[1] = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, Grid1_args.Length, Grid1_args.Length * sizeof(uint));
        Gridargsbuffer[1].SetData(Grid1_args);
        
        GridMaterial[1].renderQueue = 2000;
        
        GridMaterial[1].SetBuffer("GridBuff", GridBuffer1);
        GridMaterial[1].SetInt("Level", 1);
        
        //grid2
        RenderParams rp_grid2 = new RenderParams(GridMaterial[2])
        {
            receiveShadows = false,
            shadowCastingMode = ShadowCastingMode.Off
        };
        
        Grid2_args[0] = (uint)GridMesh[2].GetIndexCount(0);
        Grid2_args[1] = (uint)GridTotalCellCount[2];
        Grid2_args[2] = (uint)GridMesh[2].GetIndexStart(0);
        Grid2_args[3] = (uint)GridMesh[2].GetBaseVertex(0);
        Grid2_args[4] = (uint)0;
        Gridargsbuffer[2] = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, Grid2_args.Length, Grid2_args.Length * sizeof(uint));
        Gridargsbuffer[2].SetData(Grid2_args);
        
        GridMaterial[2].renderQueue = 2000;
        
        GridMaterial[2].SetBuffer("GridBuff", GridBuffer2);
        GridMaterial[2].SetInt("Level", 2);
        
        //grid3
        RenderParams rp_grid3 = new RenderParams(GridMaterial[3])
        {
            receiveShadows = false,
            shadowCastingMode = ShadowCastingMode.Off
        };
        
        Grid3_args[0] = (uint)GridMesh[3].GetIndexCount(0);
        Grid3_args[1] = (uint)GridTotalCellCount[3];
        Grid3_args[2] = (uint)GridMesh[3].GetIndexStart(0);
        Grid3_args[3] = (uint)GridMesh[3].GetBaseVertex(0);
        Grid3_args[4] = (uint)0;
        Gridargsbuffer[3] = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, Grid3_args.Length, Grid3_args.Length * sizeof(uint));
        Gridargsbuffer[3].SetData(Grid3_args);
        
        GridMaterial[3].renderQueue = 2000;
        
        GridMaterial[3].SetBuffer("GridBuff", GridBuffer3);
        GridMaterial[3].SetInt("Level", 3);
        
        //grid4
        RenderParams rp_grid4 = new RenderParams(GridMaterial[4])
        {
            receiveShadows = false,
            shadowCastingMode = ShadowCastingMode.Off
        };
        
        Grid4_args[0] = (uint)GridMesh[4].GetIndexCount(0);
        Grid4_args[1] = (uint)GridTotalCellCount[4];
        Grid4_args[2] = (uint)GridMesh[4].GetIndexStart(0);
        Grid4_args[3] = (uint)GridMesh[4].GetBaseVertex(0);
        Grid4_args[4] = (uint)0;
        Gridargsbuffer[4] = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, Grid4_args.Length, Grid4_args.Length * sizeof(uint));
        Gridargsbuffer[4].SetData(Grid4_args);
        
        GridMaterial[4].renderQueue = 2000;
        
        GridMaterial[4].SetBuffer("GridBuff", GridBuffer4);
        GridMaterial[4].SetInt("Level", 4);
        
        //grid5
        RenderParams rp_grid5 = new RenderParams(GridMaterial[5])
        {
            receiveShadows = false,
            shadowCastingMode = ShadowCastingMode.Off
        };
        
        Grid5_args[0] = (uint)GridMesh[5].GetIndexCount(0);
        Grid5_args[1] = (uint)GridTotalCellCount[5];
        Grid5_args[2] = (uint)GridMesh[5].GetIndexStart(0);
        Grid5_args[3] = (uint)GridMesh[5].GetBaseVertex(0);
        Grid5_args[4] = (uint)0;
        Gridargsbuffer[5] = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, Grid5_args.Length, Grid5_args.Length * sizeof(uint));
        Gridargsbuffer[5].SetData(Grid5_args);
        
        GridMaterial[5].renderQueue = 2000;
        
        GridMaterial[5].SetBuffer("GridBuff", GridBuffer5);
        GridMaterial[5].SetInt("Level", 5);

        #endregion
        // grid info to CS
        
        computeShader.SetVector(     "GridSideLenght",     floatToVector4(    GridSideLenght     ));
        computeShader.SetVectorArray("GridCellLenght",     floatArrToVector4( GridCellLenght     ));
        computeShader.SetVectorArray("GridTotalCellCount", floatArrToVector4( GridTotalCellCount ));
        computeShader.SetVectorArray("GridSideCellCount",  floatArrToVector4( GridSideCellCount  ));
        //Vector4[] TMPGridPoss = new [] {new Vector4(GridstartingPoss.x, GridstartingPoss.y, 0,0), new Vector4(GridstartingPoss.x, GridstartingPoss.y, 0,0), new Vector4(GridstartingPoss.x, GridstartingPoss.y, 0,0), new Vector4(GridstartingPoss.x, GridstartingPoss.y, 0,0), new Vector4(GridstartingPoss.x, GridstartingPoss.y, 0,0) };
        //computeShader.SetVectorArray("GridPossition", TMPGridPoss);
    }

    private void FixedUpdate()
    {          
        computeShader.SetVector("fixedDeltaTime", floatToVector4(Time.fixedDeltaTime));
        computeShader.SetVector("COLLISION", floatToVector4((float)Convert.ToInt32(COLLISON)));
        //Debug.Log(GridBuffer0.count);
        //Debug.Log(GridBuffer1.count);
        //Debug.Log(GridBuffer2.count);
        //Debug.Log(GridBuffer3.count);
        //Debug.Log(GridBuffer4.count);
        //Debug.Log(GridBuffer5.count);

        
        if (COMPUTE_SHADER)
        {
            computeShader.Dispatch(Wipe0_kernel, (int)MathF.Ceiling(GridTotalCellCount[0]/1024), 1, 1);
            computeShader.Dispatch(Wipe1_kernel, (int)MathF.Ceiling(GridTotalCellCount[1]/1024), 1, 1);
            computeShader.Dispatch(Wipe2_kernel, (int)MathF.Ceiling(GridTotalCellCount[2]/1024), 1, 1);
            computeShader.Dispatch(Wipe3_kernel, (int)MathF.Ceiling(GridTotalCellCount[3]/1024), 1, 1);
            computeShader.Dispatch(Wipe4_kernel, (int)MathF.Ceiling(GridTotalCellCount[4]/1024), 1, 1);
            computeShader.Dispatch(Wipe5_kernel, (int)MathF.Ceiling(GridTotalCellCount[5]/1024), 1, 1);
            
            
            computeShader.Dispatch(UptGrid0_kernel, (int)MathF.Ceiling(dotCount/1024), 1, 1);
            computeShader.Dispatch(UptGrid1_kernel, (int)MathF.Ceiling(dotCount/1024), 1, 1);
            computeShader.Dispatch(UptGrid2_kernel, (int)MathF.Ceiling(dotCount/1024), 1, 1);
            computeShader.Dispatch(UptGrid3_kernel, (int)MathF.Ceiling(dotCount/1024), 1, 1);
            computeShader.Dispatch(UptGrid4_kernel, (int)MathF.Ceiling(dotCount/1024), 1, 1);
            computeShader.Dispatch(UptGrid5_kernel, (int)MathF.Ceiling(dotCount/1024), 1, 1);


            if (Input.GetKeyDown(KeyCode.Space))
            {
                Grid_str_Csharp[] results = new Grid_str_Csharp[ (int)GridTotalCellCount[1]];
                GridBuffer0.GetData(results);

                for (int i = 0; i < GridTotalCellCount[1]; i++)
                {
                    if (i > 10000) {break;}
                    Debug.Log(i.ToString() + ":  m:" + results[i].mass.ToString() + "  MTPx:" + results[i].MassTPossition.x.ToString() + "  MTPy:" + results[i].MassTPossition.y.ToString() + "  Px:" + results[i].position.x.ToString() + "  Py:" + results[i].position.y.ToString());
                    //if (results[i].mass != 0 || results[i].position.x != 0 || results[i].position.y != 0 || results[i].MassTPossition.x != 0 || results[i].MassTPossition.y != 0) 
                }
            }

            
            for (int i = 0; i < (float)dotCount/(float)batchSize; i++)
            {
                computeShader.SetInt("DispatchOffset", i*batchSize);
                computeShader.Dispatch(dot_kernel, (int)MathF.Min(dotCount-i*batchSize, batchSize), 1, 1);
            }
        }

    }

    private void Update()
    { 
        //computeShader.SetVector("GridTargetPosition", new Vector4(cam.transform.position.x, cam.transform.position.y, 0, 0));
        //Debug.Log(GridCellLenght[4].ToString() + "   " +  GridSideCellCount[4].ToString());
        
        
        if (!resizing)
        {
            miscellaneousData[] miscData = new miscellaneousData[1];
            miscellaneousBuffer.GetData(miscData);

            dotCount  = miscData[0].dotCount;
            freeSpace = miscData[0].freeSpace;
        }

        DoItNextFrame(next_frame_id);

        // rendering obj
        if (RENDER_DOTS)
        {
            Graphics.DrawMeshInstancedIndirect(DotMesh, 0, DotMaterial, new Bounds(cam.transform.position, new Vector3(1,1,1)), dotargsbuffer, layer:10, castShadows:ShadowCastingMode.Off, receiveShadows:false);
        }
        
        // rendering The grids
        if (RENDER_GRID == 1 && !RENDER_DOTS)
        {
            Graphics.DrawMeshInstancedIndirect(GridMesh[0], 0, GridMaterial[0], new Bounds(Vector3.zero, Vector3.one * 100000f), Gridargsbuffer[0], layer:1, castShadows:ShadowCastingMode.Off, receiveShadows:false);
        }
        else if (RENDER_GRID == 2 && !RENDER_DOTS)
        {
            Graphics.DrawMeshInstancedIndirect(GridMesh[1], 0, GridMaterial[1], new Bounds(Vector3.zero, Vector3.one * 100000f), Gridargsbuffer[1], layer:1, castShadows:ShadowCastingMode.Off, receiveShadows:false);
        }
        else if (RENDER_GRID == 3 && !RENDER_DOTS)
        {
            Graphics.DrawMeshInstancedIndirect(GridMesh[2], 0, GridMaterial[2], new Bounds(Vector3.zero, Vector3.one * 100000f), Gridargsbuffer[2], layer:1, castShadows:ShadowCastingMode.Off, receiveShadows:false);
        }
        else if (RENDER_GRID == 4 && !RENDER_DOTS)
        {
            Graphics.DrawMeshInstancedIndirect(GridMesh[3], 0, GridMaterial[3], new Bounds(Vector3.zero, Vector3.one * 100000f), Gridargsbuffer[3], layer:1, castShadows:ShadowCastingMode.Off, receiveShadows:false);
        }
        else if (RENDER_GRID == 5 && !RENDER_DOTS)
        {
            Graphics.DrawMeshInstancedIndirect(GridMesh[4], 0, GridMaterial[4], new Bounds(Vector3.zero, Vector3.one * 100000f), Gridargsbuffer[4], layer:1, castShadows:ShadowCastingMode.Off, receiveShadows:false);
        }
        else if (RENDER_GRID == 6 && !RENDER_DOTS)
        {
            Graphics.DrawMeshInstancedIndirect(GridMesh[5], 0, GridMaterial[5], new Bounds(Vector3.zero, Vector3.one * 100000f), Gridargsbuffer[5], layer:1, castShadows:ShadowCastingMode.Off, receiveShadows:false);
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
                        resizing = true;

                        DotBuffer_TMP = new GraphicsBuffer(GraphicsBuffer.Target.Structured, dotCount, sizeof(float) * (3 + 2 + 2 + 1));
                        computeShader.SetBuffer(CopyBuff_kernel, "inputData_TMP", DotBuffer_TMP);
                        computeShader.SetInt("CopyID", 1);

                        next_frame_id = "add Dots ID part 1";
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
    

    void DoItNextFrame(string ID)
    {
        if (ID == "")
        {
            return;
        }
        else if (ID == "add Dots ID part 1")
        {
            computeShader.Dispatch(CopyBuff_kernel, 1, 1, 1);

            next_frame_id = "add Dots ID part 2";
            return;
        }
        else if (ID == "add Dots ID part 2")
        {
            dotCount = (int)MathF.Floor(1.25f*(dotCount + newDotAmount - freeSpace));
            freeSpace = 0;
            
            DotBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, dotCount, sizeof(float) * (3 + 2 + 2 + 1));
            

            // IMPORTANT: bind NEW buffer as destination
            computeShader.SetBuffer(CopyBuff_kernel, "inputData", DotBuffer);
            //computeShader.SetBuffer(CopyBuff1_kernel, "inputData_TMP", DotBuffer_TMP);

            // Rebind other kernels
            computeShader.SetBuffer(dot_kernel, "inputData", DotBuffer);
            computeShader.SetBuffer(change_kernel, "inputData", DotBuffer);
            DotMaterial.SetBuffer("_dotData", DotBuffer);

            // Tell copy shader direction
            computeShader.SetInt("CopyID", 0);

            next_frame_id = "add Dots ID part 3";
            return;
        }
        else if (ID == "add Dots ID part 3")
        {

            computeShader.Dispatch(CopyBuff_kernel, 1, 1, 1);

            next_frame_id = "add Dots ID part 4";
            return;
        }
        else if (ID == "add Dots ID part 4")
        {

            computeShader.Dispatch(change_kernel, 1, 1, 1);
            
            next_frame_id = "add Dots ID part 5";
            
            return;
        }
        else if (ID == "add Dots ID part 5")
        {
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
            
            
            next_frame_id = "";
            resizing = false;

            return;
        }
    }

    void RebindGPUBuffers(bool[] exc)
    {
        
        //GRAPHICS BUFFERS
        if (exc[0])
        {
            computeShader.SetBuffer(Wipe0_kernel, "Grid0", GridBuffer0);
            computeShader.SetBuffer(Wipe1_kernel, "Grid1", GridBuffer1);
            computeShader.SetBuffer(Wipe2_kernel, "Grid2", GridBuffer2);
            computeShader.SetBuffer(Wipe3_kernel, "Grid3", GridBuffer3);
            computeShader.SetBuffer(Wipe4_kernel, "Grid4", GridBuffer4);
            computeShader.SetBuffer(Wipe5_kernel, "Grid5", GridBuffer5);
            
            
            computeShader.SetBuffer(UptGrid0_kernel, "Grid0",     GridBuffer0);
            computeShader.SetBuffer(UptGrid0_kernel, "inputData", DotBuffer);
            computeShader.SetBuffer(UptGrid0_kernel, "miscData", miscellaneousBuffer);
            computeShader.SetBuffer(UptGrid0_kernel, "GridParams", GridParams);
            
            computeShader.SetBuffer(UptGrid1_kernel, "Grid1",     GridBuffer1);
            computeShader.SetBuffer(UptGrid1_kernel, "inputData", DotBuffer);
            computeShader.SetBuffer(UptGrid1_kernel, "miscData", miscellaneousBuffer);
            computeShader.SetBuffer(UptGrid1_kernel, "GridParams", GridParams);
            
            computeShader.SetBuffer(UptGrid2_kernel, "Grid2",     GridBuffer2);
            computeShader.SetBuffer(UptGrid2_kernel, "inputData", DotBuffer);
            computeShader.SetBuffer(UptGrid2_kernel, "miscData", miscellaneousBuffer);
            computeShader.SetBuffer(UptGrid2_kernel, "GridParams", GridParams);
            
            computeShader.SetBuffer(UptGrid3_kernel, "Grid3",     GridBuffer3);
            computeShader.SetBuffer(UptGrid3_kernel, "inputData", DotBuffer);
            computeShader.SetBuffer(UptGrid3_kernel, "miscData", miscellaneousBuffer);
            computeShader.SetBuffer(UptGrid3_kernel, "GridParams", GridParams);
            
            computeShader.SetBuffer(UptGrid4_kernel, "Grid4",     GridBuffer4);
            computeShader.SetBuffer(UptGrid4_kernel, "inputData", DotBuffer);
            computeShader.SetBuffer(UptGrid4_kernel, "miscData", miscellaneousBuffer);
            computeShader.SetBuffer(UptGrid4_kernel, "GridParams", GridParams);
            
            computeShader.SetBuffer(UptGrid5_kernel, "Grid5",     GridBuffer5);
            computeShader.SetBuffer(UptGrid5_kernel, "inputData", DotBuffer);
            computeShader.SetBuffer(UptGrid5_kernel, "miscData", miscellaneousBuffer);
            computeShader.SetBuffer(UptGrid5_kernel, "GridParams", GridParams);
            
            computeShader.SetBuffer(dot_kernel, "GridParams", GridParams);
            computeShader.SetBuffer(dot_kernel, "inputData", DotBuffer);
            computeShader.SetBuffer(dot_kernel, "miscData",  miscellaneousBuffer);
            computeShader.SetBuffer(dot_kernel, "Grid0", GridBuffer0);
            computeShader.SetBuffer(dot_kernel, "Grid1", GridBuffer1);
            computeShader.SetBuffer(dot_kernel, "Grid2", GridBuffer2);
            computeShader.SetBuffer(dot_kernel, "Grid3", GridBuffer3);
            computeShader.SetBuffer(dot_kernel, "Grid4", GridBuffer4);
            computeShader.SetBuffer(dot_kernel, "Grid5", GridBuffer5);
            
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

    #endregion

    private void OnApplicationQuit()
    {
        DotBuffer.Release();
        ChangeBuffer.Release();
        miscellaneousBuffer.Release();
        dotargsbuffer.Release();

        GridBuffer0.Release();
        GridBuffer1.Release();
        GridBuffer2.Release();
        GridBuffer3.Release();
        GridBuffer4.Release();
        GridBuffer5.Release();
        
        DotBuffer_TMP.Release();
    }
}
