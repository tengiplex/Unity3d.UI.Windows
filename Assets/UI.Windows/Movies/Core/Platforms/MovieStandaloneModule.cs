﻿using UnityEngine.Extensions;
using UnityEngine.UI.Windows.Components;
using UnityEngine.Video;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Movies {

	#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_TVOS
    [System.Serializable]
    public class MovieStandaloneModule : MovieModuleBase {

        public class Item {

            private readonly VideoPlayer videoPlayer;
            private bool isError;

			private System.Action onComplete;

            public Item(VideoPlayer videoPlayerProto, string path) {

                this.videoPlayer = videoPlayerProto.Spawn();
				this.isError = false;
                this.videoPlayer.errorReceived += this.OnError;
                this.videoPlayer.url = path;
                this.videoPlayer.Prepare();

            }

            public void Destroy() {

                this.videoPlayer.Stop();
                this.videoPlayer.isLooping = false;
                this.videoPlayer.errorReceived -= this.OnError;
                this.videoPlayer.Recycle();

            }

            private void OnError(VideoPlayer source, string message) {

                if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.LogErrorFormat("Video player error: {0}", message);
                this.isError = true;

            }

            public bool IsReady() {

                return this.videoPlayer.isPrepared;

            }

            public bool IsError() {

                return this.isError;

            }

            public bool IsPlaying() {

                return this.videoPlayer.isPlaying;

            }

            public Texture GetTexture() {

                return this.videoPlayer.texture;

            }

            public void SetLoop(bool loop) {

                this.videoPlayer.isLooping = loop;

            }

			public void Play(System.Action onComplete) {

                this.videoPlayer.Play();
				this.onComplete = onComplete;
				if (onComplete != null) this.videoPlayer.loopPointReached += this.OnLoopPointReached;

				//ME.Coroutines.Run(this.WaitForMovieEnd, this.videoPlayer);

            }

			private void OnLoopPointReached(VideoPlayer player) {

				this.videoPlayer.loopPointReached -= this.OnLoopPointReached;
				if (this.onComplete != null) this.onComplete.Invoke();

			}

			/*private IEnumerator<byte> WaitForMovieEnd() {

				while (this.videoPlayer.isPlaying == true) {

					yield return 0;

				}

				if (this.onComplete != null) this.onComplete.Invoke();

			}*/

            public void Pause() {

                this.videoPlayer.Pause();

            }

            public void Stop() {

				//ME.Coroutines.StopAll(this.videoPlayer);
				this.onComplete = null;
				this.videoPlayer.loopPointReached -= this.OnLoopPointReached;
                this.videoPlayer.Stop();

            }

        }

        private const int initialCapacity = 20;

        private readonly VideoPlayer videoPlayerProto;
        private readonly Dictionary<int, Item> itemsByResourceId = new Dictionary<int, Item>(MovieStandaloneModule.initialCapacity);

        public MovieStandaloneModule(VideoPlayer videoPlayerProto) {

            this.videoPlayerProto = videoPlayerProto;
            this.videoPlayerProto.CreatePool(MovieStandaloneModule.initialCapacity);

        }

        protected override System.Collections.IEnumerator LoadTexture_YIELD(ResourceAsyncOperation asyncOperation, IImageComponent component, MovieItem movieItem, ResourceBase resource) {

            var resourceId = resource.GetId();
			//if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log("LoadTexture_YIELD: " + resourceId);
			Item item;
            if (this.itemsByResourceId.TryGetValue(resourceId, out item) == false) {

                asyncOperation.SetValues(isDone: false, progress: 0.0f, asset: null);

                bool withFile = true;
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
                withFile = false;
#endif
				var resourcePath = resource.GetStreamPath(withFile);
				//if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log("NEW LoadTexture_YIELD PATH: " + resourcePath);
                item = new Item(this.videoPlayerProto, resourcePath);
                this.itemsByResourceId[resourceId] = item;

            }

            while (item.IsReady() == false) {

                if (item.IsError() == true) {

					//if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log("NEW LoadTexture_YIELD ERROR: " + item.GetTexture());
                    asyncOperation.SetValues(isDone: true, progress: 1f, asset: null);
                    yield break;

                }

                yield return 0;

            }

			yield return 0;
			//if (UnityEngine.UI.Windows.Constants.LOGS_ENABLED == true) UnityEngine.Debug.Log("NEW LoadTexture_YIELD LOAD: " + item.IsError() + " :: " + item.GetTexture());
            asyncOperation.SetValues(isDone: true, progress: 1f, asset: item.GetTexture());

        }

        public override bool IsMovie(Texture texture) {

            return true;

        }

        protected override void OnUnload(ResourceBase resource) {

            var resourceId = resource.GetId();
            Item item;
            if (this.itemsByResourceId.TryGetValue(resourceId, out item) == true) {

                this.itemsByResourceId.Remove(resourceId);
                item.Destroy();

            }

        }

        protected override void OnPlay(ResourceBase resource, Texture movie, System.Action onComplete) {

            var resourceId = resource.GetId();
            Item item;
            if (this.itemsByResourceId.TryGetValue(resourceId, out item) == true) {

				item.Play(onComplete);

            }

        }

        protected override void OnPlay(ResourceBase resource, Texture movie, bool loop, System.Action onComplete) {

            var resourceId = resource.GetId();
            Item item;
            if (this.itemsByResourceId.TryGetValue(resourceId, out item) == true) {

                item.SetLoop(loop);
				item.Play(onComplete);

            }

        }

        protected override void OnPause(ResourceBase resource, Texture movie) {

            var resourceId = resource.GetId();
            Item item;
            if (this.itemsByResourceId.TryGetValue(resourceId, out item) == true) {

                item.Pause();

            }

        }

        protected override void OnStop(ResourceBase resource, Texture movie) {

            var resourceId = resource.GetId();
            Item item;
            if (this.itemsByResourceId.TryGetValue(resourceId, out item) == true) {

                item.Stop();

            }

        }

        protected override bool IsPlaying(ResourceBase resource, Texture movie) {

            var resourceId = resource.GetId();
            Item item;
            if (this.itemsByResourceId.TryGetValue(resourceId, out item) == true) {

                return item.IsPlaying();

            } else {

                return false;

            }

        }

    }
    #endif

}