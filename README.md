Critias Tree System

The Critias tree system is an Unity addition that is used for an, currently in-development, open-world quest named 'The Unwritten Critias'. It was developed due to the poor
performance of the Unity's built-in SpeedTree implementation.

! IMPORTANT !
At runtime the managed terain's trees and foliage is turned off, so your terrain's grass will go away. In order to mitigate that I would recommend having two terrains, one for trees and the other for grass.

HOWTO:

Long story short, there are two important scripts. 'Treeifier' and 'TreeSystem'. Create an game object and add it these two scripts. The 'Treeifier' script is used for
extracting the data required for each drawn tree type and generating the indexed and optimized data at edit-time. The 'TreeSystem' script is used for using the data
that was generated at edit-time and rendering it in a very optimal manner.

The 'Treeifier', since it supports multiple tree types requires their prefabs. The prefabs can't just be any GameObjects, but are required to be SpeedTree objects at the moment.
Simply add the required tree prefabs in the array called 'Tree To Extract Prefabs' in the inspector. You also require to have a main terrain that contains all the trees
that you added previously in the 'Trees To Extract Prefabs'. Add it to the 'Main Managed Terrain' value in the inspector. Since the system can manage tileable multiple terrains
feel free to add the required count of terrain to the 'Managed Terrains' value in the inspector. The main terrain must also be contained in the 'Managed Terrains' array. After setting
the terrain you also need to provide the array called 'Cell Sizes' with the exact count of terrains that you have, and set the grid size used for each terrain. I would reccomend something
as close to 500m as possible. The cell size value must be a perfect fit with the terrain, a.k.a the terrain size % cell size must equal 0. Since you don't want to do that trial and error
use the 'Cell Info' button for proper grid sizes. Having those set you only need to set the 'System Quad' with the default system quad mesh, and the three tree shaders.

Set the shader SpeedTreeBillboard_Batch to 'Billboard Shader Batch', SpeedTreeBillboard_Master to 'Billboard Shader Master' and SpeedTree_Master to 'Tree Shader Master'. The 'Cell Holder'
is optional and will hold all generated grid data. The 'Data Store Path' is the path where the tree generated data is going to be stored. 

'Use XML Data' and 'Tree XML Store Path' are used if you have access to the SpeedTree's XML data files (which is probably not the case for you, but if you are curious, for the extended
discussion on the XML check out the forums) so you can leave those empty and false.

Now that all the data is set press 'Extract Tree Prototype Data' that will generate all the required data for the system to function. Make sure that you have an enabled 'Tree System'
component somwhere in the scene since it will be required for some operations. Not a good idea to touch the array 'Managed Prototypes' since that is auto-generated data. After the
generation completes feel free to paint your trees on the terrain, or if you already have them painted you can get to the next step.

The next step involves the generation of trees in the system. Just press 'Generate Trees' and the system should do it's job. It will gridify the terrain and add the trees to the system.
If you done everything correctly you should press 'Play' and see the system in action! 

Is this quite difficult to understand, or you just don't like to read? No probs, just check out the example...


For any extra details, suggestions or questions and plans for the future, feel free to get in touch via the forum!

Forums link: https://forum.unity3d.com/threads/free-critias-speedtree-tree-system.437520/

Asset store link: - pending -
