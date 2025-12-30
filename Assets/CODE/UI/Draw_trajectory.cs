using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using FixedUpdate = Unity.VisualScripting.FixedUpdate;

struct trajectoryPath_Csharp
{
    public Vector2 position;
}


public class Draw_trajectory : MonoBehaviour
{
    public int distance; 
    public float resolution;

    public gravity_Csharp grav;
    public ComputeShader grav_cs;

    
    Vector2 start_poss;
    Vector2 start_vell;
    float start_mass;

    private GraphicsBuffer trajectoryBuffer;
    private int kernel_id;
    public LineRenderer LR;
    trajectoryPath_Csharp[] results;
    public Vector2 Ballposs;
    
    private uint frames;
    
    private void OnEnable()
    {
        Ballposs = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        
        kernel_id = grav_cs.FindKernel("CalcTrajectory");
        
        grav_cs.SetFloat("Resolution", resolution);
        grav_cs.SetInt("Distance", distance);
        grav_cs.SetVector("CenterBallPoss", Ballposs);
        
        trajectoryBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, distance, sizeof(float) * 2);
        
        trajectoryPath_Csharp[] trajInput = new trajectoryPath_Csharp[distance];
        
        trajectoryBuffer.SetData(trajInput);
        grav_cs.SetBuffer(kernel_id, "trajPath", trajectoryBuffer);
    }

    private void FixedUpdate()
    {
        Ballposs = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        
        if (frames % 10 == 0)
        {
            grav_cs.SetFloat("Resolution", resolution);
            grav_cs.SetInt("Distance", distance);
            grav_cs.SetVector("CenterBallPoss", Ballposs);
            
            grav_cs.Dispatch(kernel_id, 1, 1, 1);
            results = new trajectoryPath_Csharp[distance];
            trajectoryBuffer.GetData(results);
            
            LR.positionCount = distance;
            for (int i = 0; i < distance; i++)
            {
                Vector2 poss = results[i].position;
                LR.SetPosition(i, new Vector3(poss.x, poss.y, 0));

            }
        }
        frames++;
    }



    private void OnApplicationQuit()
    {
        trajectoryBuffer.Release();
    }
}
