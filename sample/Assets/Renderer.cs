using UnityEngine;

public class Renderer {
	public int Id { get; set; }
	public void Play(object ev) {
		Debug.Log("Play:"+ev);
	}
	
	public Renderer(int id = 1) {
		this.Id = id;
	}
}
