using System.Collections.Generic;
using UnityEngine;

public static class PathFinder
{
    /// <summary>
    /// ������ ���� ��ã��
    /// </summary>
    /// <param name="startT">���� Ʈ������(�����). start�� end�� �ٶ󺸰� �־�� ��</param>
    /// <param name="start">���� ��ǥ</param>
    /// <param name="end">���� ��ǥ</param>
    /// <param name="radius">���� ũ��</param>
    /// <returns>�̵� ��ǥ ����</returns>
    public static Stack<Vector3> PathFinding(Transform startT, Vector3 start, Vector3 end, float radius)
    {
        Stack<Vector3> answer = new Stack<Vector3>();   // ��ȯ ����

        Dictionary<Vector3, bool> visited = new Dictionary<Vector3, bool>();    // ��ǥ, ��� �湮 ���� ��ųʸ�
        Dictionary<Vector3, Node> nodes = new Dictionary<Vector3, Node>();      // ��ǥ, ��� ��ųʸ�
        PriorityQueue<Node, float> pq = new PriorityQueue<Node, float>();       // �� ���� �Ÿ��� ��带 ������ �켱���� ť

        int moveModifier = 1;
        int counter = 0;
        int maxCount = 10000;

        // �ʱ� ��带 ����
        Node startNode = new();
        startNode.position = start;
        nodes.Add(startNode.position, startNode);
        pq.Enqueue(startNode, 0);

        // �켱���� ť�� ��尡 �ִٸ� �ݺ�
        while (pq.Count > 0 && ++counter < maxCount)
        {
            Node node = pq.Dequeue();                // ���� ���
            if (visited.ContainsKey(node.position))
                continue;
            visited.Add(node.position, true);

            // ���� ���� : ���� ��ǥ���� ��ǥ ��ǥ ���̿� ���� ����
            if (CheckPassable(node.position, end, radius))
            {
                answer.Push(end);
                // ����� �θ� ���� ������ �ݺ�
                while (nodes.ContainsKey(node.parent))
                {
                    answer.Push(node.position); // ���� ����� ��ǥ�� �����ϰ�
                    node = nodes[node.parent];  // ����� �θ� ����
                }
                return answer;                  // ����� ������ ��ȯ(��ǥ ��ǥ => ���� ��ǥ => ... => �ʱ� ��ǥ)
            }

            // �� 26���� Ž��
            // �� x, y, z�κ��� -1 ~ +1 ������ ��ǥ�� Ž��
            // z�� -1, 0�� �Ǵ� ���� �������� �ʴ� ����̹Ƿ� ��� ����
            for (int x = -1; x <= 1; x += 1)
            {
                for (int y = -1; y <= 1; y += 1)
                {
                    for (int z = -1; z <= 1; z += 1)
                    {
                        if (x == y && y == z && z == 0)
                            continue;

                        // ���� Ž���� ��ǥ
                        Vector3 findPosition = node.position + (x * startT.right + y * startT.up + z * startT.forward) * moveModifier;

                        // �̹� �湮�� ��ǥ��� �н�
                        if (visited.ContainsKey(findPosition))
                            continue;

                        // ���̿� ���� �ִٸ� �н�
                        if (!CheckPassable(node.position, findPosition, radius, ((x < 0 ? -x : x) + (y < 0 ? -y : y) + (z < 0 ? -z : z))))
                            continue;

                        float g = node.g + (x * x + y * y + z * z) * (-z + 2);                // �̵� �Ÿ� + �̵��� �Ÿ� * ����ġ (�뷫)
                        float h = Vector3.SqrMagnitude(end - findPosition);      // ���� �Ÿ� = ������� ��ǥ���� ���� �Ÿ� (�뷫)

                        // �� ��� ����
                        Node findNode = new Node(findPosition, node.position, g, h);
                        if (!nodes.ContainsKey(findPosition))           // ���� �� ��尡 ó�� �߰��� �����
                        {
                            nodes.Add(findPosition, findNode);          // ��� ��Ͽ� �� ��带 �߰��ϰ�
                            pq.Enqueue(findNode, findNode.f);           // ť�� �߰�
                        }
                        else if (nodes[findPosition].f > findNode.f)     // ���� �� ��尡 ������ �־�����, �������� �� ����Ÿ��� ���ٸ�
                        {
                            nodes[findPosition] = findNode;             // ��� ����� �� ���� �����ϰ�
                            pq.Enqueue(findNode, findNode.f);           // ť�� �߰�
                        }
                    }
                }
            }
        }

        // ���� ã�� ���ߴٸ� �״�� ��ȯ
        return answer;
    }

    /// <summary>
    /// ������ ����ü
    /// ��ǥ, �θ�(�� ��带 ����Ų ����� ��ǥ), ������� �Ÿ�, ����Ǵ� �������� �Ÿ�, �� ���� �Ÿ��� ����
    /// </summary>
    struct Node
    {
        public Vector3 position;
        public Vector3 parent;

        public float g;
        public float h;
        public float f;

        public Node(Vector3 _position, Vector3 _parent, float _g, float _h)
        {
            position = _position;
            parent = _parent;
            g = _g;
            h = _h;
            f = g + h;
        }
    }

    /// <summary>
    /// ������ �� ����
    /// </summary>
    /// <param name="start">���� ��ǥ</param>
    /// <param name="end">��ǥ ��ǥ</param>
    /// <param name="radius">���� ũ��</param>
    /// <returns>���� �Ÿ� �̳��� ���� �ִٸ� false, ���ٸ� true</returns>
    static bool CheckPassable(Vector3 start, Vector3 end, float radius, int sum = 0)
    {
        float distance;
        // x, y, z �̵����� ���� �� 1, 1.4, 1.7�� �Ÿ��� ����
        if (sum > 0)
        {
            if (sum == 1)
                distance = 1f;
            else if (sum == 2)
                distance = 1.414f;
            else
                distance = 1.732f;
        }
        else
        {
            distance = Vector3.Distance(start, end);
        }

        if (Physics.SphereCast(start, radius, (end - start).normalized, out _, distance, LayerMask.GetMask("Ground")))
        {
            return false;
        }
        return true;
    }
}