using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DT {
public class MultiThreadedChunkGenerator {
	public Queue loadedChunks; // ChunkProcessOutput
	public Queue unloadedChunks; // ChunkProcessInput
	
	private int numThreads = 0;
	private System.ComponentModel.BackgroundWorker[] BackgroundWorkers;  

	private bool[] busyThreads;

	int frameNum = 0;

	public void Update() {
		frameNum++;
		if(frameNum % 50 != 0) return;
		if(unloadedChunks.Count > 0) {
			for(int i = 0; i < numThreads; i++) {
				if(!busyThreads[i]) {
					busyThreads[i] = true;
					ChunkProcessInput inp = (ChunkProcessInput)unloadedChunks.Dequeue();
					inp.threadId = i;
					BackgroundWorkers[i].RunWorkerAsync(inp);
					//Update();
				}
			}
		}
	}

	public void QueueChunk(ChunkProcessInput info) {
		unloadedChunks.Enqueue(info);
	}

	public MultiThreadedChunkGenerator (int threads) {
		loadedChunks = new Queue();
		unloadedChunks = new Queue();

		numThreads = threads;
		busyThreads= new bool[threads];
		for(int i = 0; i < threads; i++) {
			busyThreads[i] = false;
		}

		InitializeBackgroundWorkers();
	}

	void InitializeBackgroundWorkers() {
		BackgroundWorkers = new System.ComponentModel.BackgroundWorker[numThreads];
		for(int i = 0; i < numThreads; i++) {
			BackgroundWorkers[i] = new System.ComponentModel.BackgroundWorker();
			BackgroundWorkers[i].DoWork += 
				new System.ComponentModel.DoWorkEventHandler(BackgroundWorkers_DoWork_ThreadedGenerateChunk);  
			BackgroundWorkers[i].RunWorkerCompleted +=  
				new System.ComponentModel.RunWorkerCompletedEventHandler(BackgroundWorkers_RunWorkerCompleted_ThreadedGenerateChunk);  
		}
	}

	void BackgroundWorkers_DoWork_ThreadedGenerateChunk (System.Object sender,
		System.ComponentModel.DoWorkEventArgs e) {

		ChunkProcessInput info = (ChunkProcessInput)e.Argument;
		e.Result = ChunkGenerator.CreateChunk(info);
	}

	private void BackgroundWorkers_RunWorkerCompleted_ThreadedGenerateChunk (System.Object sender,  
		System.ComponentModel.RunWorkerCompletedEventArgs e) {  

		UnityEngine.Debug.Log("Worker completed. e: " + e.ToString());
        if(e.Error != null)
        {
            UnityEngine.Debug.LogError("There was an error! " + e.Error.ToString());
        }
		else {
			ChunkProcessOutput result = (ChunkProcessOutput)e.Result;  
			loadedChunks.Enqueue(result);

			busyThreads[result.threadId] = false;

			if(unloadedChunks.Count > 0) {
				//Update();
			}
		}
	}  
}
}