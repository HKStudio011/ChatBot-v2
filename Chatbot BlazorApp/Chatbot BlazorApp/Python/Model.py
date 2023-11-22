from transformers import MT5Tokenizer,MT5ForConditionalGeneration,AutoModelForSeq2SeqLM
import numpy as np
import pandas as pd
import torch
import pytorch_lightning as pl
from pytorch_lightning.callbacks import  ModelCheckpoint
from pytorch_lightning.loggers.tensorboard import TensorBoardLogger
from sklearn.model_selection import train_test_split
from torch.utils.data import Dataset,DataLoader
from torch.optim import AdamW

class ChatBotDataset(Dataset):
    def __init__(self,device,data,tokenizer,max_length=256):
        self.data = data
        self.tokenizer = tokenizer
        self.max_length = max_length
        self.device = device

    def __len__(self):
        return len(self.data)
    def __getitem__(self,index):
        data_row = self.data.iloc[index]

        source_encoding = self.tokenizer.encode_plus(
            data_row["task"]+data_row["input"],
            max_length=self.max_length,
            padding="max_length",
            truncation=True,
            return_tensors="pt",
        )
        target_encoding = self.tokenizer.encode_plus(
            data_row["label"],
            max_length=self.max_length,
            padding="max_length",
            truncation=True,
            return_tensors="pt",
        )
        labels = target_encoding["input_ids"]
        labels[labels == self.tokenizer.pad_token_id] = -100

        return dict(
            imput=data_row["input"],
            label=data_row["label"],
            task=data_row["task"],
            input_ids=source_encoding["input_ids"].flatten(),
            attention_mask=source_encoding["attention_mask"].flatten(),
            decoder_attention_mask = target_encoding["attention_mask"].flatten(),
            labels=labels.flatten()
        )
 
class ChatBotDataModel(pl.LightningDataModule):
    def __init__(self,device,model_name,train,test,batch_size=2,max_length=256):
        super().__init__()
        self.train = train
        self.test = test
        self.tokenizer  = MT5Tokenizer.from_pretrained(model_name)
        self.batch_size = batch_size
        self.max_length = max_length
        self.device = device

    def setup(self,stage=None):
        self.train_dataset = ChatBotDataset(self.device ,self.train,self.tokenizer,self.max_length)
        self.test_dataset = ChatBotDataset(self.device ,self.test,self.tokenizer,self.max_length)

    def train_dataloader(self):
        return DataLoader(self.train_dataset,batch_size=self.batch_size,shuffle=True)

    def val_dataloader(self):
        return DataLoader(self.test_dataset,batch_size=self.batch_size)

    def test_dataloader(self):
        return DataLoader(self.test_dataset,batch_size=self.batch_size)
    
class ChatBotModel(pl.LightningModule):
    def __init__(self,device,model_name):
        super().__init__()
        self.save_hyperparameters(ignore=["device", "model_name"])
        self.model = MT5ForConditionalGeneration.from_pretrained(model_name)

    def forward(self,input_ids,attention_mask,decoder_attention_mask,labels=None):
        output = self.model(input_ids=input_ids,attention_mask=attention_mask,labels=labels,decoder_attention_mask=decoder_attention_mask)
        return output

    def training_step(self,batch,batch_idx):
        input_ids = batch["input_ids"]
        attention_mask = batch["attention_mask"]
        labels=batch["labels"]
        decoder_attention_mask=batch["decoder_attention_mask"]
        outputs = self(input_ids,attention_mask,decoder_attention_mask,labels)
        self.log("train_loss",outputs.loss, prog_bar=True, logger=True)
        return outputs.loss

    def validation_step(self,batch,batch_idx):
        input_ids = batch["input_ids"]
        attention_mask = batch["attention_mask"]
        labels=batch["labels"]
        decoder_attention_mask=batch["decoder_attention_mask"]
        outputs = self(input_ids,attention_mask,decoder_attention_mask,labels)
        self.log("val_loss",outputs.loss,prog_bar=True, logger=True,on_epoch=True)
        return outputs.loss

    def test_step(self,batch,batch_idx):
        input_ids = batch["input_ids"]
        attention_mask = batch["attention_mask"]
        labels=batch["labels"]
        decoder_attention_mask=batch["decoder_attention_mask"]
        outputs = self(input_ids,attention_mask,decoder_attention_mask,labels)
        self.log("test_loss",outputs.loss,prog_bar=True, logger=True)
        return outputs.loss

    def configure_optimizers(self):
        return AdamW(self.parameters(),lr=3e-4)
    
class ChatBot():
    def __init__(self,data_path,max_length=256,model_name = "./Model/mt5-base",model_name_ouput="./Model/mt5-base-fireturning-three-task",path_logger="training-logs", path_checkpoint="checkpoints",name_checkpoit="best_checkpoint"):
        self.device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        self.model_name = model_name
        self.model_name_ouput = model_name_ouput
        self.max_length = max_length
        self.data_path = data_path
        self.path_checkpoint = path_checkpoint
        self.name_checkpoit = name_checkpoit
        self.path_logger = path_logger
        self.trained_model = None
        self.trained_models = None
        self.tokenizer  = None

    def __ReadData__(self):
      data = pd.read_excel("data.xlsx",sheet_name='data').dropna()
      return data.sample(frac=1)


    def SaveModel(self):
      # giai phong ram
      torch.cuda.empty_cache()

      model = ChatBotModel.load_from_checkpoint(self.path_checkpoint+"/"+self.name_checkpoit+".ckpt",device=self.device,model_name=self.model_name)
      model.model.save_pretrained(self.model_name_ouput, from_pt=True)

      tokenizer = MT5Tokenizer.from_pretrained(self.model_name)
      tokenizer.save_pretrained(self.model_name_ouput, from_pt=True)

      # giai phong ram
      torch.cuda.empty_cache()

    def TrainingChatBot(self,loops =1,epochs=10,batch_size=2,is_retrain = False):
        # doc data
        data = self.__ReadData__()
        # giai phong ram
        torch.cuda.empty_cache()

        logger = TensorBoardLogger(self.path_logger, name="chatbot_mt5")
        checkpoint_callback = ModelCheckpoint(
            dirpath=self.path_checkpoint,
            filename=self.name_checkpoit,
            save_top_k=1,
            verbose = True,
            monitor="val_loss",
            mode= "min",
            save_weights_only=True
            )
        trainer = pl.Trainer(
                    logger=logger,
                    callbacks=checkpoint_callback,
                    max_epochs=epochs,
                    )
        model = 0
        if is_retrain:
          model = ChatBotModel(self.device,self.model_name_ouput)
        else:
          model = ChatBotModel(self.device,self.model_name)
        for i in range(loops):
          print("loop",i,":")

          if i > 0:
            # giai phong ram
            torch.cuda.empty_cache()
            model = ChatBotModel.load_from_checkpoint(self.path_checkpoint+"/"+self.name_checkpoit+".ckpt",device=self.device,model_name=self.model_name)

          #tach data
          train , test = train_test_split(data,test_size=0.2)
          data_model = ChatBotDataModel(self.device,self.model_name,train,test,batch_size,self.max_length)
          data_model.setup()

          trainer.fit(model=model,datamodule=data_model)
          trainer.test(model=model,datamodule=data_model)
          # giai phong ram
          torch.cuda.empty_cache()

    def InitGenerate(self):
      # giai phong ram
      torch.cuda.empty_cache()
      if self.trained_model == None:
        self.trained_model = MT5ForConditionalGeneration.from_pretrained(self.model_name_ouput).to(self.device)
      if self.tokenizer == None:
        self.tokenizer = MT5Tokenizer.from_pretrained(self.model_name_ouput)

    def InitGenerates(self):
      # giai phong ram
      torch.cuda.empty_cache()
      if self.trained_models == None:
        self.trained_models = AutoModelForSeq2SeqLM.from_pretrained(self.model_name_ouput).to(self.device)
      if self.tokenizer == None:
        self.tokenizer = MT5Tokenizer.from_pretrained(self.model_name_ouput)

    def GenerateKeyword(self,content, is_print_loss =False):
      return self.__Generate(content,"Sinh từ khoá từ nội dung: ", is_print_loss)

    def GenerateContent(self,content, is_print_loss =False):
      return self.__Generate(content,"Sinh nội dung từ từ khoá: ", is_print_loss)

    def ErrorVariation(self,content, is_print_loss =False):
      return self.__Generate(content,"Sửa lỗi chính tả hoặc lỗi đánh máy: ", is_print_loss)

    def GenerateKeywords(self,content, num_output = 10):
      return self.__Generates(content,"Sinh từ khoá từ nội dung: ", num_output)

    def GenerateContents(self,content, num_output = 10):
      return self.__Generates(content,"Sinh nội dung từ từ khoá: ", num_output)

    def ErrorVariations(self,content, num_output = 10):
      return self.__Generates(content,"Sửa lỗi chính tả hoặc lỗi đánh máy: ", num_output)

    def __Generate(self,content,task, is_print_loss =False):
      # giai phong ram
      torch.cuda.empty_cache()
      self.InitGenerate()

      source_encoding = self.tokenizer.encode_plus(
            task+content,
            max_length=self.max_length,
            padding="max_length",
            truncation=True,
            return_tensors="pt",
        ).to(self.device)

      with torch.no_grad():
          generated = self.trained_model.generate(
              input_ids=source_encoding["input_ids"],
              attention_mask = source_encoding["attention_mask"],
              max_length=self.max_length,
              return_dict_in_generate=True,
              output_scores = True
                      )
          result = self.tokenizer.decode(generated["sequences"][0], skip_special_tokens=True,clean_up_tokenization_spaces=True)
          if(is_print_loss):
            target_encoding = self.tokenizer.encode_plus(
              result,
              max_length=self.max_length,
              padding="max_length",
              truncation=True,
              return_tensors="pt",).to(self.device)
            labels = target_encoding["input_ids"]
            labels[labels == self.tokenizer.pad_token_id] = -100
            loss = self.trained_model(input_ids=source_encoding["input_ids"],
                      attention_mask=source_encoding["attention_mask"],
                      labels=labels,
                      decoder_attention_mask=target_encoding["attention_mask"]).loss


            result += "\n Loss: "+str(loss.item())
          return result

    def __Generates(self,content,task,num_output):
      # giai phong ram
      torch.cuda.empty_cache()
      self.InitGenerates()

      source_encoding = self.tokenizer.encode_plus(
            task+content,
            max_length=self.max_length,
            padding="max_length",
            truncation=True,
            return_tensors="pt",
        ).to(self.device)

      with torch.no_grad():
          generated = self.trained_models.generate(
              input_ids=source_encoding["input_ids"],
              attention_mask = source_encoding["attention_mask"],
              max_length=self.max_length,
              num_beams=num_output,
              no_repeat_ngram_size=2,
              num_return_sequences=num_output,
              early_stopping=True
                      )
          result=""
          for i in range(len(generated)):
            result += self.tokenizer.decode(generated[i], skip_special_tokens=True,clean_up_tokenization_spaces=True)+"\n"
          return result
