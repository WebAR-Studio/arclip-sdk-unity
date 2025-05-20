# ARClip SDK for Unity

The **ARClip SDK** by [WebAR Studio](https://web-ar.studio) enables fast and easy integration of WebAR into your Unity WebGL projects.

## 📦 Installation

To add the ARClip SDK to your Unity project:

1. Open **Unity** and go to `Window → Package Manager`.
2. Click the **+** button and select **Add package from Git URL...**
3. Paste the following URL and click **Add**:

https://github.com/WebAR-Studio/arclip_sdk.git


4. The library will be downloaded and added to your project.

### ⚠️ Important: Remove Existing ARLib Folder

Before proceeding, **make sure to delete any existing `ARLib` folder** from your `Assets/` directory.  
If you skip this step, you may encounter errors like:

error CS0433: The type 'ARLibTester' exists in both 'ARLib, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
and 'Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'


## 🔧 Post-Installation Setup

After importing the package:

### 1. Update WebGL Templates

- In the **Package Manager**, locate **ARClip → Samples**.
- Click **Import** (or **Reimport** if already done).
- Copy the `WebGLTemplates` folder from `Samples/ARClip/` into your project's `Assets/` folder.

### 2. Fix AR Lib Controller

After importing the WebGLTemplates, the `AR Lib Controller` component may get unassigned:

- Find the **GameObject** in your scene that previously had the `AR Lib Controller` script.
- Re-add the `AR Lib Controller` component manually.
- Assign the **Camera** in the component’s settings.

## 🧪 Sample Scenes

You can import sample scenes via the **Samples** section in the Package Manager. These provide working examples of how to use ARClip
