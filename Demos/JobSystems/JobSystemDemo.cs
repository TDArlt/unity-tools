using UnityEngine;
using System.Collections;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Jobs;

namespace unexpected
{
    public class JobSystemDemo : MonoBehaviour
    {

        #region general

        // ######################## GENERAL THINGS ######################## //
        /// <summary>For testing, you may switch the job system on and off</summary>
        [Header("General Properties")]
        [Tooltip("For testing, you may switch the job system on and off")]
        public bool UseJobSystem;


        /// <summary>Number of jobs to perform</summary>
        [Tooltip("Number of jobs to perform")]
        public int NumOfJobs = 10000;

        /// <summary>Re-initialize the system on runtime</summary>
        [Tooltip("Re-initialize the system on runtime")]
        public bool ReRun = false;



        /// <summary>Unity start method</summary>
        void Start()
        {
            ReInit();
        }

        /// <summary>Re-initializes the demo</summary>
        public void ReInit()
        {
            ReRun = false;

            if (UseTransformSample)
                InitTransformSample();
            
            if (UseMathSample)
                InitMathSample();
        }

        /// <summary>Unity update method</summary>
        void Update()
        {
            // Draw framerate
            Framerate.text = string.Format("{0:00.0} fps", (1f / Time.deltaTime));

            // Re-init, if desired
            if (ReRun)
                ReInit();

            // Update transform sample, if active
            if (UseTransformSample)
                UpdateTransformSample();

            if (UseMathSample)
                UpdateMathSample();
        }


        void LateUpdate()
        {
            // Update transform sample, if active
            if (UseTransformSample)
                LateUpdateTransformSample();
        }


        /// <summary>Unity OnDestroy method</summary>
        private void OnDestroy()
        {
            // Clean up transform sample
            CleanUpTransformSample();

            // Clean up math sample
            CleanUpMathSample();
        }


        #endregion



        // ######################## TRANSFORM SAMPLE ######################## //

        #region transform-sample

        #region general
        /// <summary>Enables/disables the transformation sample</summary>
        [Header("Transform sample")]
        public bool UseTransformSample = true;

        /// <summary>Define, if you like to see the objects</summary>
        public bool ObjectsVisible = true;

        /// <summary>Factor to see that this is performing in realtime</summary>
        [Range(.1f, 10)]
        public float RotationSpeed = 1;

        /// <summary>The framerate to be shown on the UI</summary>
        [Tooltip("The framerate to be shown on the UI")]
        public UnityEngine.UI.Text Framerate;


        /// <summary>These are all the transforms that are currently in the scene</summary>
        private Transform[] allTransforms;



        #endregion

        #region general-init

        /// <summary>This is the sample for the transform update</summary>
        public void InitTransformSample()
        {
            // Clean up
            CleanUpTransformSample();

            // Placing the cubes..
            GameObject[] objs = PlaceRandomCubes(NumOfJobs, NumOfJobs / 100f);
            allTransforms = new Transform[NumOfJobs];

            // Connect them

            for (int i = 0; i < NumOfJobs; i++)
            {
                allTransforms[i] = objs[i].transform;

                if (!ObjectsVisible)
                    objs[i].GetComponent<Renderer>().enabled = false;
            }

            // ...and specific stuff
            SpecificInitTransformSample();
        }


        /// <summary>This places randomly cubes</summary>
        /// <param name="count">the amount</param>
        /// <param name="radius">the area where to place</param>
        /// <returns></returns>
        public static GameObject[] PlaceRandomCubes(int count, float radius)
        {
            var cubes = new GameObject[count];
            var cubeToCopy = MakeStrippedCube();

            for (int i = 0; i < count; i++)
            {
                var cube = GameObject.Instantiate(cubeToCopy);
                cube.transform.position = UnityEngine.Random.insideUnitSphere * radius;
                cubes[i] = cube;
            }

            Destroy(cubeToCopy);

            return cubes;
        }

        /// <summary>Creates a single cube</summary>
        /// <returns>the game object of it</returns>
        public static GameObject MakeStrippedCube()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //turn off shadows entirely
            var renderer = cube.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            // disable collision
            var collider = cube.GetComponent<Collider>();
            collider.enabled = false;

            return cube;
        }

        #endregion

        #region specific-properties


        /// <summary>This is a transform access class that is shared throughout multiple threads</summary>
        private TransformAccessArray transformAccesses;

        /// <summary>This is an array that can be shared and transferred to single threads</summary>
        private NativeArray<Vector3> randomDirections;


        /// <summary>This is a single job that can run in multiple threads</summary>
        private PositionUpdateJob jobForTransformSample;

        /// <summary>This is a handler to manage all the current jobs</summary>
        private JobHandle jobHandleForTransformSample;

        #endregion
        
        #region specific-init

        /// <summary>This is the init, that has some specific properties for multithreading</summary>
        private void SpecificInitTransformSample()
        {

            // Create random rotation directions for each cube (that's not the interesting stuff)
            Vector3[] directions = new Vector3[NumOfJobs];
            for (int i = 0; i < NumOfJobs; i++)
                directions[i] = new Vector3(0, Random.Range(-90, 90), Random.Range(-90, 10));


            // Now, transfer these properties to a shared space in memory:
            randomDirections = new NativeArray<Vector3>(directions, Allocator.Persistent);

            // And we also need to access the transform in other threads, so create a shared space here as well:
            transformAccesses = new TransformAccessArray(allTransforms);
        }

        #endregion
        
        #region specific-update
        

        /// <summary>This is what should run on every update</summary>
        private void UpdateTransformSample()
        {

            // JOB SYSTEM ON:
            if (UseJobSystem)
            {
                // Only run a new job, if the old one has finished
                // (we only need this, because we want flexibility for measuring here, otherwise you would not do this here!)

                if (jobHandleForTransformSample.IsCompleted)
                {
                    // Create a job (using the struct, see below)
                    jobForTransformSample = new PositionUpdateJob()
                    {
                        Speed = RotationSpeed,
                        RotationDirections = randomDirections,
                        DeltaTime = Time.deltaTime
                    };

                    // Plan our job for all the transform-nodes in our array
                    jobHandleForTransformSample = jobForTransformSample.Schedule(transformAccesses);

                    // If this does not depend on anything, the jobHandle will start working just in this moment.

                }
            }
            else
            // JOB SYSTEM OFF:
            {
                WorkNoJobSystemTransformSample();
            }

        }


        /// <summary>This is what should happen in late update</summary>
        private void LateUpdateTransformSample()
        {
            // This is meant to wait for all jobs to be completed, in order to measure the framerate
            if (!jobHandleForTransformSample.IsCompleted)
                jobHandleForTransformSample.Complete();
        }

        #endregion
        
        #region specific-work
        



        /// <summary>This is the work that is done when not using the job system</summary>
        private void WorkNoJobSystemTransformSample()
        {
            for (int i = 0; i < allTransforms.Length; ++i)
            {
                allTransforms[i].localRotation = Quaternion.Euler(allTransforms[i].localRotation.eulerAngles + RotationSpeed * randomDirections[i] * Time.deltaTime);
            }
        }



        /// <summary>This is a job that can work in parallel (multithreaded) for transform nodes</summary>
        struct PositionUpdateJob : IJobParallelForTransform
        {
            /// <summary>
            /// This is an array we get, placed in shared memory with simplified options (doesn't use all the Unity-overload)
            /// In order to don't mess up on shared memory, it's generally a good idea to define every
            /// parameter "ReadOnly" if you don't need to change it here.
            /// </summary>
            [ReadOnly]
            public NativeArray<Vector3> RotationDirections;

            /// <summary>This is just a var, we transfer. As simple data types don't eat up memory, we don't need to work shared here/summary>
            public float Speed;

            /// <summary>This is another var for keeping track of the frame time/summary>
            public float DeltaTime;

            /// <summary>
            /// This is what runs when performing the job.
            /// For this type of interface, the handler spawns a job for each single transform and trys to run them in parallel
            /// (shifting on CPU-cores)
            /// </summary>
            /// <param name="i">is the current position at our array</param>
            /// <param name="transform">is the current transform we are looking at</param>
            public void Execute(int i, TransformAccess transform)
            {
                // This is just what should happen here
                transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles + RotationDirections[i] * Speed * DeltaTime);
            }
        }


        #endregion
        
        #region specific-cleanup


        /// <summary>
        /// This cleans up everything for the transform sample.
        /// Should be called at destroy and before re-initializing
        /// </summary>
        public void CleanUpTransformSample()
        {

            // Complete all open jobs
            jobHandleForTransformSample.Complete();

            // If we have objects in scene, we also habe jobs
            if (allTransforms != null && allTransforms.Length > 0)
            {
                // Remove all objects
                for (int i = allTransforms.Length - 1; i >= 0; --i)
                {
                    if (allTransforms != null && allTransforms[i] != null)
                        Destroy(allTransforms[i].gameObject);
                }

                // Dispose shared data (as this is in shared memory, it is not cleaned up by itself)
                transformAccesses.Dispose();
                randomDirections.Dispose();
            }
        }


        #endregion

        #endregion


        // ######################## MATH SAMPLE ######################## //

        #region math-sample


        #region general

        /// <summary>Defines, if you like to use the math sample</summary>
        [Header("Math sample")]
        public bool UseMathSample = false;
        
        /// <summary>These are the first vectors</summary>
        private Vector3[] vecsA;

        /// <summary>These are the second vectors</summary>
        private Vector3[] vecsB;

        /// <summary>Tells you, if the math sample has been completed</summary>
        private bool completedMathSample = true;

        /// <summary>This stores the start time of the job performing</summary>
        private System.DateTime startTime;
        

        #endregion

        #region specific-properties

        /// <summary>The handler for the math job preparation</summary>
        private JobHandle jobHandlerPrepareMathSample = new JobHandle();
        /// <summary>The handler for the main math jobs</summary>
        private JobHandle jobHandlerMainMathSample = new JobHandle();

        /// <summary>This is the job for preparing</summary>
        private MathJobPrepare prepareJobMathSample;
        /// <summary>This is the job for preparing</summary>
        private MathJobMain mainJobMathSample;


        /// <summary>This is the shared memory space for the first vectors</summary>
        private NativeArray<Vector3> vecA;

        /// <summary>This is the shared memory space for the second vectors</summary>
        private NativeArray<Vector3> vecB;

        #endregion

        #region specific


        /// <summary>Initializes and starts the math sample</summary>
        public void InitMathSample()
        {
            CleanUpMathSample();

            // Initialize arrays
            vecsA = new Vector3[NumOfJobs];
            vecsB = new Vector3[NumOfJobs];

            for (int i = 0; i < NumOfJobs; i++)
            {
                vecsA[i] = new Vector3();
                vecsB[i] = new Vector3();
            }

            // Push to shared memory
            vecA = new NativeArray<Vector3>(vecsA, Allocator.Persistent);
            vecB = new NativeArray<Vector3>(vecsB, Allocator.Persistent);


            // Store values
            startTime = System.DateTime.Now;
            completedMathSample = false;


            // Start correct job
            if (UseJobSystem)
                WorkJobSystemMathSample();
            else
                StartCoroutine(WorkNoJobSystemMathSample());

        }

        


        #endregion

        #region jobs


        /// <summary>No-job work</summary>
        private IEnumerator WorkNoJobSystemMathSample()
        {
            startTime = System.DateTime.Now;
            yield return null;

            // STEP ONE: Generate random values for vecA and vecB

            for (int i = 0; i < vecsA.Length; i++)
            {
                vecsA[i] = new Vector3(Random.Range(-10,10), Random.Range(-10, 10), Random.Range(-10, 10));
                vecsB[i] = new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10));
            }


            // STEP TWO: Make a math operation for each vecA with vecB
            for (int i = 0; i < vecsA.Length; i++)
            {
                for (int j = 0; j < vecsB.Length; j++)
                {
                    float val = Vector3.Magnitude(Vector3.Cross(vecA[i], vecB[j]));
                }
            }


            yield return null;

            // If not using job system, this job has finished in one frame. Print

            float timeUsed = ((float)(System.DateTime.Now - startTime).TotalMilliseconds) / 1000f;
            Debug.LogFormat("Finished without job system in {0:0.00} seconds.", timeUsed);
            completedMathSample = true;
        }




        /// <summary>Job work</summary>
        private void WorkJobSystemMathSample()
        {
            // Prepare-job
            prepareJobMathSample = new MathJobPrepare()
            {
                VecA = vecA,
                VecB = vecB
            };

            jobHandlerPrepareMathSample = prepareJobMathSample.Schedule(vecsA.Length, 64);


            // Main job

            mainJobMathSample = new MathJobMain()
            {
                VecA = vecA,
                VecB = vecB
            };

            jobHandlerPrepareMathSample = mainJobMathSample.Schedule(vecsA.Length, 1, jobHandlerPrepareMathSample);
            

            // Now, everything runs.
        }






        /// <summary>
        /// We'll find the end of the jobs in the update function
        /// </summary>
        private void UpdateMathSample()
        {
            // Only check, if not yet finished
            if (!completedMathSample)
            {
                if (UseJobSystem)
                {
                    // When using job systems, wait for them to be completed
                    if (jobHandlerPrepareMathSample.IsCompleted && jobHandlerMainMathSample.IsCompleted)
                    {
                        StartCoroutine(PrintFinishTimeMathSample());
                        completedMathSample = true;
                        CleanUpMathSample();
                    }
                }
            }
        }


        private IEnumerator PrintFinishTimeMathSample()
        {
            // Wait one frame
            yield return null;

            float timeUsed = ((float)(System.DateTime.Now - startTime).TotalMilliseconds) / 1000f;
            Debug.LogFormat("Finished with job system in {0:0.00} seconds.", timeUsed);
        }



        #endregion

        #region job-definitions

        /// <summary>This is the job to prepare the arrays</summary>
        struct MathJobPrepare : IJobParallelFor
        {
            // Get access to the vectors

            /// <summary>First vector</summary>
            public NativeArray<Vector3> VecA;
            /// <summary>Second vector</summary>
            public NativeArray<Vector3> VecB;

            public void Execute(int index)
            {
                VecA[index] = new Vector3(-1.5f, 2.7f, 7.3f);
                VecB[index] = new Vector3(9.25f, 1.1f, 1);
            }
        }

        /// <summary>This is the job to do the maths</summary>
        struct MathJobMain : IJobParallelFor
        {
            // Get access to the vectors

            /// <summary>First vector</summary>
            [ReadOnly]
            public NativeArray<Vector3> VecA;
            /// <summary>Second vector</summary>
            [ReadOnly]
            public NativeArray<Vector3> VecB;

            public void Execute(int index)
            {
                for (int i = 0; i < VecB.Length; i++)
                {
                    float val = Vector3.Magnitude(Vector3.Cross(VecA[index], VecB[i]));
                }
            }
        }


        #endregion

        #region specific-cleanup


        /// <summary>
        /// This cleans up everything for the math sample.
        /// Should be called at destroy and before re-initializing
        /// </summary>
        public void CleanUpMathSample()
        {
            jobHandlerPrepareMathSample.Complete();
            jobHandlerMainMathSample.Complete();

            if (vecA.IsCreated)
                vecA.Dispose();
            if (vecB.IsCreated)
                vecB.Dispose();
        }


        #endregion



        #endregion
    }
}