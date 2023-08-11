import argparse
from peft import PeftModel
import torch
import torch.nn.functional as F
from transformers import LlamaTokenizer, LlamaForCausalLM

parser = argparse.ArgumentParser()
parser.add_argument('--model_path', type=str, default='openlm-research/open_llama_7b')
parser.add_argument('--load_in_4bit', type=bool, default=True)
parser.add_argument('--use_lora', type=bool, default=False)
parser.add_argument('--lora_path', type=str, default='./loras/open_llama_7b_demo')
args = parser.parse_args()

tokenizer = LlamaTokenizer.from_pretrained(args.model_path)
model = LlamaForCausalLM.from_pretrained(args.model_path, torch_dtype=torch.float16, device_map='auto', load_in_4bit=args.load_in_4bit)

if args.use_lora:
    print(f"TSRACT: Using LoRA {args.lora_path}.")
    model = PeftModel.from_pretrained(model, args.lora_path)

print("TSRACT: Ready.")

while True:
    prompt = input()

    # Determine mode from first character
    mode = prompt[0]
    # Determine max tokens if in generation mode
    max_new_tokens = int(prompt[1:prompt.index('"')]) if mode == "G" else 1
    # Remove mode indicator, quotes from beginning and end
    prompt = prompt[prompt.index('"')+1: -1]
    prompt = prompt.replace("\\n", "\n")
    inputs = tokenizer([prompt], return_tensors="pt")
    inputs = inputs.to('cuda')
    outputs = model.generate(**inputs, max_new_tokens=max_new_tokens, return_dict_in_generate=True, output_scores=True)

    if mode == "P":
        # Probability dump mode
        softmax_scores = F.softmax(outputs.scores[0], dim=1)
        sorted_scores, sorted_indices = torch.sort(softmax_scores[0], descending=True)

        for token_id, prob in zip(sorted_indices, sorted_scores):
            print(f"{token_id}\t{prob.item()}")

    elif mode == "G":
        # Text generation mode
        for token_id in outputs.sequences[0][inputs.input_ids.shape[1]:]: # slice from end of input to end of output
            print(f"{token_id}")

    print("TSRACT: Finished")
