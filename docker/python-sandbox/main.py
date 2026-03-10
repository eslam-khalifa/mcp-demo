import sys
import json

def main():
    """Execute AI-generated Python code in a sandboxed environment.

    Expects JSON on stdin: {"code": "<python source>", "data": "<json string>"}
    The code can use: json, pandas (as pd), numpy (as np).
    The variable `data` is pre-populated with the parsed JSON payload.
    All output must be printed to stdout.
    """
    try:
        import pandas as pd
        import numpy as np

        # Set up a result collector if the user wants to return data
        # but the standard way is to use print()

        raw = sys.stdin.read()
        if not raw:
            return

        input_payload = json.loads(raw)

        code = input_payload.get("code", "")
        data_str = input_payload.get("data", "null")

        if not code:
            print(json.dumps({"error": "No code provided"}))
            sys.exit(1)

        # Parse the data payload so the AI code can use it directly
        try:
            data = json.loads(data_str) if data_str else None
        except json.JSONDecodeError:
            print(json.dumps({"error": "Provided data is not valid JSON"}))
            sys.exit(1)

        # Restricted namespace — only safe builtins + analytics libraries
        exec_globals = {
            "__builtins__": {
                "print": print, "len": len, "range": range, "enumerate": enumerate,
                "zip": zip, "map": map, "filter": filter, "sorted": sorted,
                "min": min, "max": max, "sum": sum, "abs": abs, "round": round,
                "int": int, "float": float, "str": str, "bool": bool,
                "list": list, "dict": dict, "tuple": tuple, "set": set,
                "isinstance": isinstance, "type": type,
                "True": True, "False": False, "None": None,
            },
            "json": json,
            "pd": pd,
            "np": np,
            "data": data,
        }

        # Execute the code
        exec(code, exec_globals)

    except Exception as e:
        # Catch and report any other errors (syntax, runtime, etc.)
        # Use a dictionary to ensure it's valid JSON for the caller
        print(json.dumps({"error": f"{type(e).__name__}: {str(e)}"}))
        sys.exit(1)

if __name__ == "__main__":
    main()
