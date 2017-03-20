using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DT {
public class ThreadedChunkGenerator {
	public Queue loadedChunks; // Chunk
	public Queue unloadedChunks; // ChunkProcess

	private System.ComponentModel.BackgroundWorker BackgroundWorker1;  

	public ThreadedChunkGenerator () {
		UnityEngine.Debug.Log("ThreadedChunkGenerator created");

		loadedChunks = new Queue();
		unloadedChunks = new Queue();

		BackgroundWorker1 = new System.ComponentModel.BackgroundWorker();
		InitializeBackgroundWorker();
	}

	public void QueueChunk(ChunkProcessInput info) {
		//info.sampler = (Vector3 position) => { return Mathf.Sin(position.x) + Mathf.Sin(position.z); };
		UnityEngine.Debug.Log("queuechunk called");
		if(BackgroundWorker1.IsBusy) {
			unloadedChunks.Enqueue(info);
		}
		else {
			BackgroundWorker1.RunWorkerAsync(info);
		}
	}

	void InitializeBackgroundWorker() {
		BackgroundWorker1.DoWork +=  
			new System.ComponentModel.DoWorkEventHandler(BackgroundWorker1_DoWork_ThreadedGenerateChunk);  
		BackgroundWorker1.RunWorkerCompleted +=  
			new System.ComponentModel.RunWorkerCompletedEventHandler(BackgroundWorker1_RunWorkerCompleted_ThreadedGenerateChunk);  
	}

	void BackgroundWorker1_DoWork_ThreadedGenerateChunk (System.Object sender,
		System.ComponentModel.DoWorkEventArgs e) {
		ChunkProcessInput info = (ChunkProcessInput)e.Argument;
		e.Result = ChunkGenerator.CreateChunk(info);
	}

	// Executed when done
	private void BackgroundWorker1_RunWorkerCompleted_ThreadedGenerateChunk (  
		object sender,  
		System.ComponentModel.RunWorkerCompletedEventArgs e)  
	{  
		UnityEngine.Debug.Log("Worker completed. e: " + e.ToString());
		//UnityEngine.Debug.Log("Worker completed. e.Error: " + e.Error.ToString());
		//UnityEngine.Debug.Log("Worker completed. e.Result: " + e.Result.ToString());
        if(e.Error != null)
        {
            UnityEngine.Debug.LogError("There was an error! " + e.Error.ToString());
        }
		else {
			// Access the result through the Result property.  
			ChunkProcessOutput result = (ChunkProcessOutput)e.Result;  
			UnityEngine.Debug.Log("Got here!");
			loadedChunks.Enqueue(result);
			UnityEngine.Debug.Log("loadedChunks count: ");
			UnityEngine.Debug.Log(loadedChunks.Count);

			// Queue other unloaded chunks if they exist
			if(unloadedChunks.Count > 0) {
				QueueChunk((ChunkProcessInput)unloadedChunks.Dequeue());
			}
		}
	}  
}
}