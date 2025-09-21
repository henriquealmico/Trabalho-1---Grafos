import collections
import numpy as np
from representations import GraphRepresentation, AdjacencyList, AdjacencyMatrix
from typing import Type, Optional, Dict, List, Tuple

class Graph:
    """
    Classe principal para manipulação de grafos. Abstrai a representação
    subjacente e fornece uma interface de alto nível para o usuário.
    """
    def __init__(self, num_vertices: int, representation_cls: Type[GraphRepresentation] = AdjacencyList):
        if num_vertices <= 0:
            raise ValueError("O número de vértices deve ser um inteiro positivo.")
        
        self._num_vertices = num_vertices
        self._representation = representation_cls(num_vertices)
        self._all_degrees_cache = None # Cache para os graus

    def _invalidate_degree_cache(self):
        """Invalida o cache de graus sempre que uma aresta é adicionada."""
        self._all_degrees_cache = None

    @property
    def vertex_count(self) -> int:
        return self._num_vertices

    @property
    def edge_count(self) -> int:
        return self._representation.edge_count

    def add_edge(self, u: int, v: int):
        if not (GraphUtils.is_vertex_within_bounds(u, self._num_vertices) and GraphUtils.is_vertex_within_bounds(v, self._num_vertices)):
            raise ValueError(f"Vértices {u} e {v} estão fora do intervalo [1, {self._num_vertices}]")
        
        self._representation.add_edge(u, v)
        self._invalidate_degree_cache()

    def get_neighbors(self, v: int) -> List[int]:
        """Retorna uma lista de vizinhos do vértice v."""
        if not GraphUtils.is_vertex_within_bounds(v, self._num_vertices):
            raise ValueError(f"Vértice {v} está fora do intervalo [1, {self._num_vertices}]")
        
        return self._representation.get_neighbors(v)

    # --- Métricas de Grau ---

    def _calculate_all_degrees(self) -> List[int]:
        """Calcula e cacheia os graus de todos os vértices."""
        if self._all_degrees_cache is None:
            self._all_degrees_cache = [len(self.get_neighbors(i)) for i in range(1, self.vertex_count + 1)]
        return self._all_degrees_cache

    def get_degree_metrics(self) -> Dict[str, float]:
        """
        Retorna um dicionário com o grau mínimo, máximo, médio e a mediana de grau. [cite: 107]
        """
        degrees = self._calculate_all_degrees()
        sorted_degrees = sorted(degrees)
        
        n = len(sorted_degrees)
        median = 0
        if n > 0:
            if n % 2 == 1:
                median = sorted_degrees[n // 2]
            else:
                mid1 = sorted_degrees[n // 2 - 1]
                mid2 = sorted_degrees[n // 2]
                median = (mid1 + mid2) / 2

        return {
            "min_degree": min(degrees) if degrees else 0,
            "max_degree": max(degrees) if degrees else 0,
            "avg_degree": np.mean(degrees) if degrees else 0,
            "median_degree": median
        }
        
    # --- Algoritmos de Busca ---

    def breadth_first_search(self, start_vertex: int) -> Tuple[Dict[int, Optional[int]], Dict[int, int]]:
        """
        Executa a Busca em Largura (BFS) a partir de um vértice inicial.

        Args:
            start_vertex: O vértice para iniciar a busca.

        Returns:
            Uma tupla contendo dois dicionários:
            - parents: Mapeia cada vértice ao seu pai na árvore de busca.
            - levels: Mapeia cada vértice ao seu nível (distância da raiz).
        """
        if not (GraphUtils.is_vertex_within_bounds(start_vertex, self.vertex_count)):
            raise ValueError(f"Vértice inicial {start_vertex} está fora do intervalo.")

        parents = {v: None for v in range(1, self.vertex_count + 1)}
        levels = {v: -1 for v in range(1, self.vertex_count + 1)}
        
        queue = collections.deque([start_vertex])
        levels[start_vertex] = 0
        
        while queue:
            u = queue.popleft()
            for v in self.get_neighbors(u):
                if levels[v] == -1: # Se o vértice não foi visitado
                    levels[v] = levels[u] + 1
                    parents[v] = u
                    queue.append(v)
        
        return parents, levels

    def depth_first_search(self, start_vertex: int) -> Tuple[Dict[int, Optional[int]], Dict[int, int]]:
        """
        Executa a Busca em Profundidade (DFS) a partir de um vértice inicial. [cite: 112]

        Args:
            start_vertex: O vértice para iniciar a busca.

        Returns:
            Uma tupla contendo dois dicionários:
            - parents: Mapeia cada vértice ao seu pai na árvore de busca.
            - levels: Mapeia cada vértice à sua profundidade na árvore de busca.
        """
        if not (1 <= start_vertex <= self.vertex_count):
            raise ValueError(f"Vértice inicial {start_vertex} está fora do intervalo.")
            
        parents = {v: None for v in range(1, self.vertex_count + 1)}
        levels = {v: -1 for v in range(1, self.vertex_count + 1)}
        stack = [(start_vertex, 0)] # Armazena (vértice, nível)
        
        visited = {start_vertex}
        parents[start_vertex] = None
        levels[start_vertex] = 0
        
        while stack:
            u, level = stack.pop()
            
            for v in sorted(self.get_neighbors(u), reverse=True): # Ordem consistente
                if v not in visited:
                    visited.add(v)
                    parents[v] = u
                    levels[v] = level + 1
                    stack.append((v, level + 1))
                    
        return parents, levels

    # --- Distância e Diâmetro ---

    def get_distance(self, u: int, v: int) -> int:
        """
        Calcula a distância (caminho mínimo) entre dois vértices. 
        Retorna -1 se não houver caminho.
        """
        _, levels = self.breadth_first_search(u)
        return levels[v]

    def get_diameter(self) -> int:
        """
        Calcula o diâmetro exato do grafo. [cite: 115, 116]
        Pode ser computacionalmente caro para grafos grandes.
        """
        max_dist = 0
        for i in range(1, self.vertex_count + 1):
            _, levels = self.breadth_first_search(i)
            current_max = max(levels.values())
            if current_max > max_dist:
                max_dist = current_max
        return max_dist

    # --- Componentes Conexas ---
    
    def get_connected_components(self) -> List[Dict]:
        """
        Encontra todas as componentes conexas do grafo. [cite: 117]
        Retorna uma lista de dicionários, cada um representando uma componente,
        ordenada por tamanho em ordem decrescente. 
        """
        visited = set()
        components = []
        for v in range(1, self.vertex_count + 1):
            if v not in visited:
                component_nodes = set()
                queue = collections.deque([v])
                visited.add(v)
                component_nodes.add(v)
                
                while queue:
                    u = queue.popleft()
                    for neighbor in self.get_neighbors(u):
                        if neighbor not in visited:
                            visited.add(neighbor)
                            component_nodes.add(neighbor)
                            queue.append(neighbor)
                
                components.append({
                    "size": len(component_nodes),
                    "vertices": sorted(list(component_nodes))
                })
        
        # Ordena as componentes por tamanho, em ordem decrescente
        return sorted(components, key=lambda x: x['size'], reverse=True)

    # --- Método de Fábrica ---

    @staticmethod
    def from_file(file_path: str, representation_cls: Type[GraphRepresentation] = AdjacencyList):
        try:
            with open(file_path, 'r') as f:
                num_vertices = int(f.readline().strip())
                graph = Graph(num_vertices, representation_cls)

                for line in f:
                    parts = line.strip().split()
                    if len(parts) == 2:
                        u, v = int(parts[0]), int(parts[1])
                        graph.add_edge(u, v)
                return graph
        except FileNotFoundError:
            raise FileNotFoundError(f"Arquivo não encontrado em: {file_path}")
        except ValueError:
            raise ValueError("Formato de arquivo inválido. Verifique o conteúdo.")
        

class GraphUtils:

    @staticmethod
    def is_vertex_within_bounds(v: int, num_vertices: int) -> bool:
        """Verifica se um vértice está dentro dos limites válidos."""
        return 1 <= v <= num_vertices