using UnityEngine;

public class Renderer {
	public int Id { get; set; }
	public void Play(object ev) {
		Debug.Log("Play:"+ev);
	}
	
	public Renderer() {
		this.Id = 1;
	}
}
