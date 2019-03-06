using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            string sample = "MIIEvwIBADANBgkqhkiG9w0BAQEFAASCBKkwggSlAgEAAoIBAQC9rGtAXTfglq2acKsGYDs+zLoW7LJbiU8D2cfrOzXFrgEX8AtwuyFOUiKsj7HwhbfcjRIdiMKR7sv1l0bzOxAlaGVDEnoSJ3tyDhnvC9Uqeh5vLc0s9LG+8/e6cUhdeTY0RUt23fGFgK8OIZ2eYkmNqTtoIV21C8xDBmjMYNS21jHgdw9KWQIqEyeBTXNeIVcZuP4r/gbtplhfnpCe7mqHmGZTygxn4r/fDPQ3IGx/sNmTu5i1npdw4fP2xZCjCkSTz106EmRM9TXypzIvDHi8ycFM/uByKyUuKkdq8fIQOijAgMg4VSXmaAzrvu2yJPIBYm1fYBwQCK090+ivf2avAgMBAAECggEAf0dWaUikmHdEY+C7Q3oMB6ZGMeAAB+DqPwFDYJzJBrAvV6rjYnCQdwgy8G000NxKdxvLTjpZpqgCAfnTyKCXwyJ84Tdi5w/LjMvdp0Xfc7Oi/KRVjJdfN25rjJc8Ik2WjBj7/PYOfrHNxsPUC1aVWRR5IvVQ9o7GMSv54zwPQejsdxJKteZ0bUhPOzgcNMfd6051nXLFRdLSYiiWRqGlEAD1lbZqOmCzF7QJz9OJh7wM2jtMF7ihw3REVFT/B3sE1pMGmb/3P3bLLhxDxKJnlPiO0sZ2/Gp4S9kmoBgPmYnBPKbXbVTfxZg07UCPc97Bc1rJuJOw1hdyrkeaotcMAQKBgQD5BrBrus0fOAnSoLv79O5GL3OxXo4+SZzAaB6/rC6wZvtFFkqm/3eJaDywfWjpMpPZ1VSxqd0kCEfK+jSqNK2+H9RNEm+oOAC3cbjWw4zBwt0Y4DxaEo6fMo1+gokgkns9oKexhgwTacFyUrvEKAWDTl2qAcpvb5k5DIiNWJeRAQKBgQDC/DiDK8oWuz55omQil6m7FQTDE3NUlogwhTDPT61kdGER+TFk0jey/JBi/0VjZ+eF5R98yx0xuQ+nf6wudWGiX5WbNF5yjZpPXu1wJkAK2abIX/ggfNMn73kNW01bL4aT0TpV2rGW3hKd9ydvadZ5jJYcDDuFSf2QxpSXqqxHrwKBgQDF7jUnS1BiMe5MxYjk2GbSzkCMh/VTOLsoixl1i2uItjGdVfx8A62FP56NQQCz6YluqIsqszKbwyEdCgX2CuzVowLhR3gMIocfR3p86OzlzPZjIUeW6A0IJ+wi06oeg48FCr5+8WaDv5kMPwoS/SR0m0MDL20xaWhF2dpnTjUaAQKBgQC5Q+l2SN8lphgAfpnifHRbO+dga1TD5JvWblcoQ66eqi2pZDrYbx1ZRbzzM1V81DcZ89BtRJiirBIBtr+lDQcNvwBpjeLHuWALVkkIrG9hX9imvvkF9VS0t0cvt7bSk1+th7mD5d2jWbIawcGIjOmqaDggwkazqM/zBZweV56GJQKBgQCEFPU4tshsObee1IngYqcBlOygw0LmBUhbImayzOoUpw/NhNKQHKPyxpa4ylOdlwpXHis4MyjoqwxnZE+znrNYN8kyVP4GbZ6c2YnY4+YYu/IpRythNuivaIh+ZkJgZqLYZ+5h0MYwoQGem1/pJPdaeaaIDRsbYlhwiGwW2IXmvQ==";

            // 生成编码表
            var nodeList = (from d in sample
                            group d by d into g
                            select new Node
                            {
                                key = g.Key,
                                weight = g.Count()
                            }).ToList();

            // 生成哈夫曼二叉树
            while (nodeList.Count(p => p.parent == null) > 1)
            {
                var twoNodeList = nodeList.Where(p => p.parent == null).OrderBy(p => p.weight).Take(2).ToList();

                Node newNode = new Node
                {
                    weight = twoNodeList.Sum(p => p.weight),
                    left = twoNodeList.OrderBy(p => p.weight).First(),
                    right = twoNodeList.OrderBy(p => p.weight).Last()
                };

                twoNodeList.ForEach(p => p.parent = newNode);
                nodeList.Add(newNode);
            }

            SetPath(nodeList.First(p => p.parent == null), string.Empty);
            nodeList = nodeList.Where(p => p.key != null).ToList();

            string bitList = string.Empty;
            foreach (var s in sample)
            {
                bitList += nodeList.First(p => p.key == s).path;
            }

            // 补位对齐
            int number = bitList.Length % 8;

            for (int i = 0; i < number; i++)
            {
                bitList += "0";
            }

            //压缩字节数组
            byte[] result = new byte[bitList.Length / 8];
            for (int i = 0; i < bitList.Count(); i += 8)
            {
                result[i / 8] = Convert.ToByte(string.Join(string.Empty, bitList.Take(8)), 2);
                bitList = bitList.Remove(0, 8);
            }

            // 编码结果表
            foreach (var node in nodeList)
            {
                Console.WriteLine(string.Format("{0}:{1}", node.key, node.path));
            }
            Console.WriteLine(string.Format("补齐位数：{0}", number));
            Console.WriteLine(string.Format("压缩字节数：{0}", result.Count()));

            Console.ReadLine();
        }
        // 计算编码
        static void SetPath(Node node, string path)
        {
            node.path = path;

            if (node.left != null)
            {
                SetPath(node.left, path + "0");
            }
            if (node.right != null)
            {
                SetPath(node.right, path + "1");
            }
        }

        class Node
        {
            public Node parent;

            public char? key;
            public int weight;
            public string path = string.Empty;

            public Node left;
            public Node right;
        }
    }
}
